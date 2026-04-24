using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.EF;

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