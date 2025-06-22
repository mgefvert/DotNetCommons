namespace DotNetCommons.EF;

/// <summary>
/// Defines the interface for a CRUD (Create, Read, Update, Delete) service
/// that provides standardized operations for working with data objects.
/// </summary>
/// <typeparam name="TDataKey">The type of the primary key for the data object.</typeparam>
/// <typeparam name="TDataObject">The type of data object being managed.</typeparam>
/// <typeparam name="TListQuery">The type of query object used for filtering list operations.</typeparam>
public interface ICrudService<TDataKey, TDataObject, in TListQuery>
    where TListQuery : class
{
    /// <summary>
    /// Creates new objects in the database based on the provided items.
    /// </summary>
    /// <param name="items">Array of objects to create.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Array of keys for the newly created objects.</returns>
    Task<TDataKey[]> Create(TDataObject[] items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple objects by their IDs.
    /// </summary>
    /// <param name="ids">Array of object IDs to retrieve.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Array of found objects matching the provided IDs.</returns>
    Task<TDataObject[]> Get(TDataKey[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single object by its ID.
    /// </summary>
    /// <param name="id">The ID of the object to retrieve.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The found object.</returns>
    Task<TDataObject> Get(TDataKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists objects from the database based on optional query parameters.
    /// </summary>
    /// <param name="query">Optional query parameters to filter the results.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Array of objects matching the query criteria.</returns>
    Task<TDataObject[]> List(TListQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing objects in the database with the values from the provided items.
    /// </summary>
    /// <param name="items">Array of objects with updated values. Each object must have a valid key.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Array of keys for the successfully updated objects.</returns>
    Task<TDataKey[]> Update(TDataObject[] items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes objects from the database by their IDs.
    /// </summary>
    /// <param name="ids">Array of object IDs to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Array of keys for the successfully deleted objects.</returns>
    Task<TDataKey[]> Delete(TDataKey[] ids, CancellationToken cancellationToken = default);
}
