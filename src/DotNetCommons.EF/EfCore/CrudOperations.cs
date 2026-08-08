using System.Linq.Expressions;
using DotNetCommons.EF.ObjectManagement;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.EF.EfCore;

public class CrudOperations<T, TKey>
    where T : class, new()
    where TKey : struct
{
    private readonly DbContext _dbContext;
    private readonly DbSet<T> _dbSet;
    private readonly Expression<Func<T, TKey>> _keyExpression;
    private readonly Func<T, TKey> _keySelector;
    private readonly Func<IQueryable<T>, IQueryable<T>> _selectionFilter;

    public Error DuplicateObjects { get; set; } = new(ErrorCategory.InvalidData, "Duplicate request object ID.");
    public Error InvalidObject { get; set; }    = new(ErrorCategory.InvalidData, "Invalid request object.");
    public Error NullObject { get; set; }       = new(ErrorCategory.InvalidData, "Request object cannot be null.");
    public Error ObjectNotChanged { get; set; } = new(ErrorCategory.NoOp, "Object not changed.");
    public Error ObjectNotFound { get; set; }   = new(ErrorCategory.NotFound, "Object not found.");

    public CrudOperations(
        DbContext dbContext,
        Expression<Func<T, TKey>> keySelector,
        Func<IQueryable<T>, IQueryable<T>> selectionFilter)
    {
        _dbContext            = dbContext;
        _dbSet                = dbContext.Set<T>();
        _keyExpression        = keySelector;
        _selectionFilter = selectionFilter;
        _keySelector          = keySelector.Compile();
    }

    private sealed class WorkItem
    {
        public T? Request { get; set; }
        public T? DbObject { get; set; }
        public TKey? Key { get; set; }
        public Result<TKey>? Result { get; set; }

        public WorkItem(T? request, T? dbObject, TKey? key)
        {
            DbObject = dbObject;
            Request  = request;
            Key      = key;
        }
    }

    public async Task<Results<TKey>> Create(ICollection<T> objects, Action<T> populateCallback, Action<T>? postCallback = null)
    {
        var dbSet = _dbContext.Set<T>();
        var patch = new Patch();

        // Create new objects for everything and zip together with the requests
        var batch = objects.Select(o => new WorkItem(o, new T(), null)).ToList();

        foreach (var item in batch)
        {
            // Is the request object valid? Do we have a validator?
            if (item.Request == null!)
            {
                item.Result = Result<TKey>.Fail(NullObject);
                continue;
            }

            if (item.Request is IValidation { IsValid: false })
            {
                item.Result = Result<TKey>.Fail(InvalidObject);
                continue;
            }

            patch.UpdateObject(item.DbObject!, item.Request);
            populateCallback(item.DbObject!);
            dbSet.Add(item.DbObject!);
        }

        await _dbContext.SaveChangesAsync();

        // Assign back the IDs into the result
        foreach (var item in batch.Where(item => item.Result == null!))
        {
            postCallback?.Invoke(item.DbObject!);
            item.Result = Result<TKey>.Ok(_keySelector(item.DbObject!));
        }

        var results = new Results<TKey>();
        results.AddRange(batch.Select(x => x.Result));
        return results;
    }

    public Task<List<T>> GetObjects(ICollection<TKey> ids, Func<IQueryable<T>, IQueryable<T>>? filter = null)
    {
        var baseQuery = _selectionFilter(_dbContext.Set<T>());
        var idList    = ids.Distinct().Order().ToList();
        var predicate = DbSetExtensions.PredicatePropertyExistsInCollection(_keyExpression, idList);
        var query     = baseQuery.Where(predicate);

        if (filter != null)
            query = filter(query);

        return query.ToListAsync();
    }

    public async Task<Results<T>> Get(ICollection<TKey> ids, Func<IQueryable<T>, IQueryable<T>>? filter = null)
    {
        var result = await GetObjects(ids, filter);

        var results = new Results<T>();
        results.AddRange(result.Select(Result<T>.Ok));
        return results;
    }

    public async Task<Result<List<T>>> List(Func<IQueryable<T>, IQueryable<T>>? filter = null)
    {
        var query = _selectionFilter(_dbContext.Set<T>());

        if (filter != null)
            query = filter(query);

        return Result<List<T>>.Ok(await query.ToListAsync());
    }

    public async Task<Results> Update(ICollection<T> objects)
    {
        var ids = objects.Select(_keySelector).NotNulls().Distinct().ToList();
        if (ids.Count != objects.Count)
            return Results.FailEverything(DuplicateObjects);

        // Start off by building a work list for the objects to be updated
        var batch = objects.Select(o => new WorkItem(o, null, _keySelector(o))).ToList();

        // Are the request objects valid? Do we have validators?
        foreach (var item in batch)
        {
            switch (item.Request)
            {
                case null:
                    item.Result = Result<TKey>.Fail(NullObject);
                    continue;
                case IValidation { IsValid: false }:
                    item.Result = Result<TKey>.Fail(InvalidObject);
                    break;
            }
        }

        // Rebuild the list of IDs and load the objects
        ids = batch.Where(x => x.Result == null).Select(x => x.Key!.Value).NotNulls().ToList();

        var existing = await GetObjects(ids);
        var lookup   = existing.ToDictionary(_keySelector);
        var patch    = new Patch();

        foreach (var item in batch.Where(x => x.Result == null))
        {
            item.DbObject = lookup.GetValueOrDefault(item.Key!.Value);
            if (item.DbObject == null)
            {
                item.Result = Result<TKey>.Fail(ObjectNotFound);
                continue;
            }

            if (!patch.UpdateObject(item.DbObject, item.Request!))
            {
                item.Result = Result<TKey>.Fail(ObjectNotChanged);
                continue;
            }

            item.Result = Result<TKey>.Ok(default);
        }

        await _dbContext.SaveChangesAsync();

        var results = new Results();
        results.AddRange(batch.Select(x => x.Result!.IsSuccess ? Result.Ok() : Result.Fail(x.Result.Error!)));

        return results;
    }

    public async Task<Results> Delete(ICollection<TKey> ids)
    {
        var batch = ids.Select(id => new WorkItem(null, null, id)).ToList();

        ids = batch.Where(x => x.Result == null).Select(x => x.Key!.Value).NotNulls().ToList();

        var existing = await GetObjects(ids);
        var lookup   = existing.ToDictionary(_keySelector);

        foreach (var item in batch)
        {
            item.DbObject = lookup.GetValueOrDefault(item.Key!.Value);
            if (item.DbObject == null)
            {
                item.Result = ObjectNotFound;
                continue;
            }

            _dbSet.Remove(item.DbObject);
            item.Result = Result<TKey>.Ok(default);
        }

        await _dbContext.SaveChangesAsync();

        var results = new Results();
        results.AddRange(batch.Select(x => x.Result!.IsSuccess ? Result.Ok() : Result.Fail(x.Result.Error!)));

        return results;
    }
}