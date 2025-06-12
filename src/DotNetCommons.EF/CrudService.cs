using System.Net;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace DotNetCommons.EF;

/// <summary>
/// Defines the available CRUD operations that can be performed by a service.
/// This enum is marked with the [Flags] attribute, allowing combinations of operations.
/// </summary>
[Flags]
public enum CrudOperations
{
    /// <summary>
    /// No operations are allowed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Allows retrieving individual items by ID.
    /// </summary>
    Get = 1,

    /// <summary>
    /// Allows listing multiple items with optional filtering.
    /// </summary>
    List = 2,

    /// <summary>
    /// Allows read-only operations (Get and List).
    /// </summary>
    ReadOnly = 3,

    /// <summary>
    /// Allows creation of new items.
    /// </summary>
    Create = 4,

    /// <summary>
    /// Allows updating existing items.
    /// </summary>
    Update = 8,

    /// <summary>
    /// Allows deletion of items.
    /// </summary>
    Delete = 16,

    /// <summary>
    /// Allows all CRUD operations.
    /// </summary>
    All = 31,
}

/// <summary>
/// Provides a base implementation for CRUD (Create, Read, Update, Delete) operations
/// on entities within a database context. Includes access control and validation hooks.
/// </summary>
/// <typeparam name="TDataKey">The type of the primary key for the data object.</typeparam>
/// <typeparam name="TDataObject">The type of data object being managed.</typeparam>
/// <typeparam name="TDataContext">The type of Entity Framework DbContext.</typeparam>
/// <typeparam name="TListQuery">The type of query object used for filtering list operations.</typeparam>
public abstract class CrudService<TDataKey, TDataObject, TDataContext, TListQuery>
        : ICrudService<TDataKey, TDataObject, TListQuery>
    where TDataKey : notnull
    where TDataObject : class, new()
    where TDataContext : DbContext
    where TListQuery : class
{
    // ReSharper disable once StaticMemberInGenericType
    protected static readonly (PropertyInfo, UpdateableAttribute)[] PropertyMap;

    /// <summary>
    /// Gets the database context used for database operations.
    /// </summary>
    protected TDataContext Context { get; }

    /// <summary>
    /// Gets or sets the operations that are allowed for this service instance.
    /// Controls which CRUD operations can be performed through permission flags.
    /// </summary>
    protected CrudOperations AllowedOperations { get; init; }

    static CrudService()
    {
        PropertyMap = typeof(TDataObject).GetProperties()
            .Where(p => p is { CanRead: true, CanWrite: true })
            .Select(p => (Prop: p, Attr: p.GetCustomAttribute<UpdateableAttribute>()!))
            .Where(p => p.Attr != null!)
            .ToArray();
    }

    protected CrudService(TDataContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Assigns values from the source object to the existing object. This allows an end user to update properties on an object from API
    /// methods. Properties that should not be touched are typically object keys, relational keys, system attributes (CreatedAt, UpdatedAt,
    /// CreatedBy...), password hashes and similar. The default method searches for properties marked with the
    /// <see cref="UpdateableAttribute"/> attribute and allows operations only on those.
    /// </summary>
    /// <param name="existing">The existing object to which values will be assigned.</param>
    /// <param name="source">The source object from which values will be copied.</param>
    /// <returns>The updated existing object with values assigned from the source object.</returns>
    protected virtual bool UpdateObject(TDataObject existing, TDataObject source)
    {
        ArgumentNullException.ThrowIfNull(existing, nameof(existing));
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        var updated = false;
        foreach (var (prop, attr) in PropertyMap)
        {
            var sourceValue = prop.GetValue(source);
            var targetValue = prop.GetValue(existing);

            if (sourceValue == null && attr.NullRemovesValue == false)
                continue;

            if (sourceValue == null && targetValue == null)
                continue;

            if (targetValue == sourceValue || (sourceValue?.Equals(targetValue) ?? false))
                continue;

            prop.SetPropertyValue(existing, sourceValue);
            updated = true;
        }

        return updated;
    }

    /// <summary>
    /// Constructs a query to load data objects based on specified identifiers or query parameters.
    /// </summary>
    /// <param name="ids">An optional array of identifiers to filter the data objects.</param>
    /// <param name="query">An optional query parameter for defining additional filters or criteria.</param>
    /// <returns>An <see cref="IQueryable{TDataObject}"/> containing the filtered data objects based on the specified criteria.</returns>
    protected abstract IQueryable<TDataObject> LoadQuery(TDataKey[]? ids = null, TListQuery? query = null);

    /// <summary>
    /// Retrieves the unique key for a given data object.
    /// </summary>
    /// <param name="value">The data object from which the key will be extracted.</param>
    /// <returns>The unique key associated with the provided data object.</returns>
    protected abstract TDataKey GetObjectKey(TDataObject value);

    /// <summary>
    /// Creates a new instance of the data object.
    /// </summary>
    /// <returns>A new instance of the data object.</returns>
    protected virtual TDataObject NewObject() => new();

    /// <summary>
    /// Verifies whether the specified item is allowed to be created based on the defined rules and constraints.
    /// </summary>
    /// <param name="item">The item to check for permission or validation before creation.</param>
    protected virtual void VerifyCanCreate(TDataObject item)
    {
    }

    /// <summary>
    /// Verifies whether a read operation can be performed on the specified data object.
    /// </summary>
    /// <param name="item">The data object on which the read operation is being performed.</param>
    protected virtual void VerifyCanRead(TDataObject item)
    {
    }

    /// <summary>
    /// Verifies whether the specified data object can be updated based on custom be updated logic
    /// or based on business rules.
    /// </summary>
    /// <param name="item">The data object to be updated.</param>
    protected virtual void VerifyCanUpdate(TDataObject item)
    {
    }

    /// <summary>
    /// Verifies whether the specified data object can be deleted based on custom be updated logic
    /// or based on business rules.
    /// </summary>
    /// <param name="item">The data object to be deleted.</param>
    protected virtual void VerifyCanDelete(TDataObject item) {}

    /// <inheritdoc/>
    public virtual async Task<TDataKey[]> Create(TDataObject[] items, CancellationToken cancellationToken = default)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.Create))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "Create operation not allowed");
        
        var save = items.Select(source =>
        {
            var item = NewObject();
            UpdateObject(item, source);
            VerifyCanCreate(item);
            return item;
        }).ToArray();
        
        Context.AddRange(save.Cast<object>());
        await Context.SaveChangesAsync(cancellationToken);

        return save.Select(GetObjectKey).ToArray();
    }
    
    /// <inheritdoc/>
    public virtual async Task<TDataObject[]> Get(TDataKey[] ids, CancellationToken cancellationToken = default)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.Get))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "Get operation not allowed");
        
        var result = await LoadQuery(ids).ToArrayAsync(cancellationToken);
        foreach (var item in result)
            VerifyCanRead(item);

        return result;
    }

    /// <inheritdoc/>
    public virtual async Task<TDataObject> Get(TDataKey id, CancellationToken cancellationToken = default)
    {
        return
            (await Get([id], cancellationToken))
            .SingleOrDefault() ?? throw new AppException(HttpStatusCode.NotFound, $"Object {id} not found");
    }

    /// <inheritdoc/>
    public Task<TDataObject[]> List(TListQuery? query = null, CancellationToken cancellationToken = default)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.List))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "List operation not allowed");
        
        return LoadQuery(null, query).ToArrayAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<TDataKey[]> Update(TDataObject[] items, CancellationToken cancellationToken = default)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.Update))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "Update operation not allowed");
        
        // Get a list of existing items, securely loaded
        var existing = await Get(items.Select(GetObjectKey).ToArray(), cancellationToken);

        // Intersect with the list of given items
        var updates = existing.Intersect(items, GetObjectKey, GetObjectKey);

        // For every item found in both lists, update the existing one, then save
        foreach (var update in updates.Both)
        {
            UpdateObject(update.Item1, update.Item2);
            VerifyCanUpdate(update.Item1);
        }

        await Context.SaveChangesAsync(cancellationToken);
        
        return updates.Both.Select(u => GetObjectKey(u.Item1)).ToArray();
    }

    /// <inheritdoc/>
    public virtual async Task<TDataKey[]> Delete(TDataKey[] ids, CancellationToken cancellationToken = default)
    {
        if (!AllowedOperations.HasFlag(CrudOperations.Delete))
            throw new AppException(HttpStatusCode.MethodNotAllowed, "Delete operation not allowed");
        
        var items = await LoadQuery(ids).ToArrayAsync(cancellationToken);
        foreach (var item in items)
            VerifyCanDelete(item);

        Context.RemoveRange(items.Cast<object>());
        await Context.SaveChangesAsync(cancellationToken);
        
        return items.Select(GetObjectKey).ToArray();
    }
}