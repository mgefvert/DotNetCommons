using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.EF;

public class DbConcurrentList<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class, new()
{
    private readonly ConcurrentDictionary<string, int> _cache;
    private readonly Func<TEntity, int> _idSelector;
    private readonly Func<TEntity, string> _valueSelector;
    private readonly Action<TEntity, string> _valueSetter;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly IDbContextFactory<TContext> _contextFactory;
    private readonly Expression<Func<TEntity, string>> _valueExpression;

    public DbConcurrentList(
        IDbContextFactory<TContext> contextFactory,
        Expression<Func<TEntity, int>> idProperty,
        Expression<Func<TEntity, string>> valueProperty)
    {
        _contextFactory = contextFactory;

        _idSelector      = idProperty.Compile();
        _valueExpression = valueProperty;
        _valueSelector   = valueProperty.Compile();
        _valueSetter     = CreateSetter(valueProperty);

        _cache = new ConcurrentDictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
    }

    public async Task LoadInitialData(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var             dataset = context.Set<TEntity>();

        var query = ApplyFilter(dataset.AsNoTracking());
        var data  = query.ToList();
        foreach (var item in data)
            _cache.TryAdd(_valueSelector(item), _idSelector(item));
    }

    public bool TryGetValue(string name, out int id)
    {
        return _cache.TryGetValue(name, out id);
    }

    public async Task<int> GetOrAddAsync(string name)
    {
        if (TryGetValue(name, out var id))
            return id;

        await _writeLock.WaitAsync();
        try
        {
            // Double-check in cache (another thread may have added it)
            if (TryGetValue(name, out id))
                return id;

            // We do not have it in the cache. Start interacting with the database.
            await using var context = await _contextFactory.CreateDbContextAsync();
            var             dataset = context.Set<TEntity>();

            // Check database for the value
            var lookupId = await AddFromDatabase(dataset, name);
            if (lookupId != null)
                return lookupId.Value;

            // Not in database, create new entry
            try
            {
                return await CreateNewEntity(context, dataset, name);
            }
            catch (Exception e)
            {
                // This likely means that we have hit a unique index, and someone created it just before us.
            }

            // Check database for the value again, to see if it exists now
            lookupId = await AddFromDatabase(dataset, name);
            if (lookupId != null)
                return lookupId.Value;

            throw new Exception($"Failed to create or retrieve ID for '{name}' after multiple attempts.");
        }
        finally
        {
            _writeLock.Release();
        }
    }

    protected virtual IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query)
    {
        return query;
    }

    protected virtual void ConfigureNewEntity(TEntity entity, string name)
    {
    }

    private async Task<int?> AddFromDatabase(DbSet<TEntity> dataset, string name)
    {
        // Try to load from the database
        var query = dataset.Where(DbSetExtensions.PredicatePropertyEqualTo(_valueExpression, name));
        query = ApplyFilter(query);
        var existingItem = await query.FirstOrDefaultAsync();

        if (existingItem == null)
            return null;

        // Found, update the internal cache
        var id = _idSelector(existingItem);
        _cache.TryAdd(name, id);
        return id;
    }

    private async Task<int> CreateNewEntity(DbContext context, DbSet<TEntity> dataset, string name)
    {
        var newItem = new TEntity();
        _valueSetter(newItem, name);
        ConfigureNewEntity(newItem, name);

        dataset.Add(newItem);
        await context.SaveChangesAsync();

        var newId = _idSelector(newItem);
        _cache.TryAdd(name, newId);
        return newId;
    }

    private static Action<TEntity, string> CreateSetter(Expression<Func<TEntity, string>> valueProperty)
    {
        var memberExpression = (MemberExpression)valueProperty.Body;
        var parameter        = Expression.Parameter(typeof(TEntity));
        var valueParameter   = Expression.Parameter(typeof(string));
        var assign = Expression.Assign(
            Expression.Property(parameter, memberExpression.Member.Name),
            valueParameter);
        var result = Expression.Lambda<Action<TEntity, string>>(assign, parameter, valueParameter);
        return result.Compile();
    }
}