using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.EF.EfCore;

/// <summary>
/// Represents a thread-safe, in-memory cache for entities with a specific discriminator value managed by a DbContext.
/// Enables filtering based on the provided discriminator property and value, and allows creating new entities if they
/// do not exist in the database, incorporating the associated discriminator.
/// </summary>
/// <typeparam name="TContext">The type of the DbContext used to access the database.</typeparam>
/// <typeparam name="TEntity">The type of the entity managed by this list.</typeparam>
/// <typeparam name="TDiscriminator">The type of the discriminator used to filter entities.</typeparam>
public class DbConcurrentListDiscriminator<TContext, TEntity, TDiscriminator>
    : DbConcurrentList<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class, new()
    where TDiscriminator : struct
{
    private readonly Expression<Func<TEntity, TDiscriminator>> _discriminatorExpression;
    private readonly TDiscriminator _discriminatorValue;
    private readonly Action<TEntity, TDiscriminator> _discriminatorSetter;

    public DbConcurrentListDiscriminator(
        IDbContextFactory<TContext> contextFactory,
        Expression<Func<TEntity, int>> idProperty,
        Expression<Func<TEntity, string>> valueProperty,
        Expression<Func<TEntity, TDiscriminator>> discriminatorProperty,
        TDiscriminator discriminatorValue)
        : base(contextFactory, idProperty, valueProperty)
    {
        _discriminatorExpression = discriminatorProperty;
        _discriminatorValue      = discriminatorValue;
        _discriminatorSetter     = CreateDiscriminatorSetter(discriminatorProperty);
    }

    protected override IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query)
    {
        return query.Where(DbSetExtensions.PredicatePropertyEqualTo(_discriminatorExpression, _discriminatorValue));
    }

    protected override void ConfigureNewEntity(TEntity entity, string name)
    {
        _discriminatorSetter(entity, _discriminatorValue);
    }

    private static Action<TEntity, TDiscriminator> CreateDiscriminatorSetter(Expression<Func<TEntity, TDiscriminator>> discriminatorProperty)
    {
        var memberExpression = (MemberExpression)discriminatorProperty.Body;
        var parameter        = Expression.Parameter(typeof(TEntity));
        var valueParameter   = Expression.Parameter(typeof(TDiscriminator));
        var assign           = Expression.Assign(Expression.Property(parameter, memberExpression.Member.Name), valueParameter);
        var result           = Expression.Lambda<Action<TEntity, TDiscriminator>>(assign, parameter, valueParameter);

        return result.Compile();
    }
}