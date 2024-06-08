using System.Net;
using Microsoft.EntityFrameworkCore;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace DotNetCommons.EF;

[Flags]
public enum CrudOperations
{
    None = 0,
    Get = 1,
    List = 2,
    ReadOnly = 3,
    Create = 4,
    Update = 8,
    Delete = 16,
    All = 31,
}

public abstract class CrudService<TDataKey, TDataObject, TDataContext, TListQuery> : ICrudService<TDataKey, TDataObject, TListQuery>
    where TDataKey : notnull
    where TDataObject : class, new()
    where TDataContext : DbContext
    where TListQuery : class
{
    protected TDataContext Context { get; }
    protected CrudOperations AllowedOperations { get; set; }

    protected CrudService(TDataContext context)
    {
        Context       = context;
    }

    protected abstract TDataObject AssignObject(TDataObject existing, TDataObject source);
    protected abstract IQueryable<TDataObject> LoadQuery(TDataKey[]? ids = null, TListQuery? query = null);
    protected abstract TDataKey GetObjectKey(TDataObject value);

    protected virtual TDataObject NewObject() => new();
    protected virtual void VerifyCanCreate(TDataObject item) {}
    protected virtual void VerifyCanRead(TDataObject item) {}
    protected virtual void VerifyCanUpdate(TDataObject item) {}
    protected virtual void VerifyCanDelete(TDataObject item) {}
    
    public virtual async Task<TDataKey[]> Create(TDataObject[] items)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.Create))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "Create operation now allowed");
        
        var save = items.Select(source =>
        {
            var item = AssignObject(NewObject(), source);
            VerifyCanCreate(item);
            return item;
        }).ToArray();
        
        Context.AddRange(save.Cast<object>());
        await Context.SaveChangesAsync();

        return save.Select(GetObjectKey).ToArray();
    }
    
    public virtual async Task<TDataObject[]> Get(TDataKey[] ids)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.Get))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "Get operation not allowed");
        
        var result = await LoadQuery(ids).ToArrayAsync();
        foreach (var item in result)
            VerifyCanRead(item);

        return result;
    }

    public virtual async Task<TDataObject> Get(TDataKey id)
    {
        return (await Get([id])).SingleOrDefault() ?? throw new AppException(HttpStatusCode.NotFound, $"Object {id} not found");
    }

    public Task<TDataObject[]> List(TListQuery? query = null)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.List))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "List operation not allowed");
        
        return LoadQuery(null, query).ToArrayAsync();
    }

    public virtual async Task<TDataKey[]> Update(TDataObject[] items)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.Update))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "Update operation not allowed");
        
        // Get a list of existing items, securely loaded
        var existing = await Get(items.Select(GetObjectKey).ToArray());

        // Intersect with the list of given items
        var updates = existing.Intersect(items, GetObjectKey, GetObjectKey);

        // For every item found in both lists, update the existing one, then save
        foreach (var update in updates.Both)
        {
            AssignObject(update.Item1, update.Item2);
            VerifyCanUpdate(update.Item1);
        }

        await Context.SaveChangesAsync();
        
        return updates.Both.Select(u => GetObjectKey(u.Item1)).ToArray();
    }

    public virtual async Task<TDataKey[]> Delete(TDataKey[] ids)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.Delete))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "Delete operation not allowed");
        
        var items = await LoadQuery(ids).ToArrayAsync();
        foreach (var item in items)
            VerifyCanDelete(item);

        Context.RemoveRange(items.Cast<object>());
        await Context.SaveChangesAsync();
        
        return items.Select(GetObjectKey).ToArray();
    }
}