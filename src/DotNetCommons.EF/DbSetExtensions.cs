using System.Linq.Expressions;
using DotNetCommons.EF.DataSeeding;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.EF;

public static class DbSetExtensions
{
    /// <summary>
    /// Seeds data into the specified DbSet using the provided seeding logic. Provides a callback object which can be used
    /// to ensure creation of records and enums.
    /// </summary>
    /// <remarks>
    /// It is the responsibility of the caller to eventually call DbContext.SaveChanges() to persist the changes.
    /// </remarks>
    /// <param name="dbSet">The DbSet to which the data will be seeded.</param>
    /// <param name="seedAction">A delegate that defines the seeding logic using the <see cref="DbContextSeeder{T}" />.</param>
    public static void Seed<T>(this DbSet<T> dbSet, Action<DbContextSeeder<T>> seedAction)
        where T : class, new()
    {
        var seeder = new DbContextSeeder<T>(dbSet);
        seedAction(seeder);
    }

    /// <summary>
    /// Loads records from the database that match the specified property values from the provided collection.
    /// </summary>
    /// <param name="dbSet">The DbSet representing the database table to query.</param>
    /// <param name="seedKeySelector">An expression used to select the key property to match in the database records.</param>
    /// <param name="values">A collection of objects containing the key values to search for in the database.</param>
    /// <typeparam name="T">The type of the entities in the DbSet.</typeparam>
    /// <typeparam name="TKey">The type of the key property used for matching.</typeparam>
    /// <returns>A list of records from the database that match the specified property values.</returns>
    public static List<T> LoadByPropertySelector<T, TKey>(
        this DbSet<T> dbSet,
        Expression<Func<T, TKey>> seedKeySelector,
        ICollection<TKey> values
    )
        where T : class
    {
        var predicate = PredicatePropertyExistsInCollection(seedKeySelector, values);

        return dbSet.Where(predicate).ToList();
    }

    /// <summary>
    /// Creates an expression that checks if a specified property of an entity is equal to a given value.
    /// </summary>
    /// <remarks>
    /// This function takes an expression like e => e->Id, and generates an expression e => e-Id == value.
    /// </remarks>
    public static Expression<Func<TEntity, bool>> PredicatePropertyEqualTo<TEntity, TProp>(
        Expression<Func<TEntity, TProp>> propertySelector,
        TProp value)
    {
        var parameter = propertySelector.Parameters[0];

        // Ensure the constant matches the property type (important for nullables/conversions)
        Expression constant = Expression.Constant(value, typeof(TProp));
        if (constant.Type != propertySelector.Body.Type)
            constant = Expression.Convert(constant, propertySelector.Body.Type);

        var body = Expression.Equal(propertySelector.Body, constant);
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    /// <summary>
    /// Generates a predicate expression that checks if the specified property values exist within a given collection.
    /// </summary>
    /// <param name="propertySelector">An expression that selects the property from the entity to match against the collection.</param>
    /// <param name="values">The collection of values to be checked for existence against the selected property.</param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProp">The type of the property being selected and matched.</typeparam>
    /// <returns>An expression that represents a predicate to check if the selected property exists in the given collection.</returns>
    public static Expression<Func<TEntity, bool>> PredicatePropertyExistsInCollection<TEntity, TProp>(
        Expression<Func<TEntity, TProp>> propertySelector, ICollection<TProp> values)
        where TEntity : class
    {
        var parameter    = Expression.Parameter(typeof(TEntity), "entity");
        var selectorBody = ReplaceParameter(propertySelector.Body, propertySelector.Parameters[0], parameter);

        var containsMethod = typeof(Enumerable)
            .GetMethods()
            .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TProp));

        var containsCall = Expression.Call(null, containsMethod, Expression.Constant(values), selectorBody);
        var predicate    = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameter);

        return predicate;
    }

    /// Replaces occurrences of a specific parameter in the given expression with another parameter.
    private static Expression ReplaceParameter(Expression body, ParameterExpression from, ParameterExpression to)
    {
        return new ParameterReplaceVisitor(from, to).Visit(body);
    }

    /// Represents a custom expression visitor that replaces occurrences of a specific parameter
    /// in an expression with another parameter.
    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to   = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : base.VisitParameter(node);
        }
    }
}