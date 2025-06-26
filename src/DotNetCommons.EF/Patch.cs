using System.Collections.Concurrent;
using System.Reflection;

namespace DotNetCommons.EF;

/// <summary>
/// Specifies the modes of operation for the patching functionality, determining the allowed operations
/// such as creating new objects, removing existing objects, or both.
/// </summary>
[Flags]
public enum PatchMode
{
    /// <summary>
    /// Allows the creation of new objects during a patch operation.
    /// When this mode is enabled, objects present in the load list but absent in the existing list
    /// will be created and added to the existing list.
    /// </summary>
    AllowNewObjects = 1,

    /// <summary>
    /// Allows the removal of existing objects during a patch operation.
    /// When this mode is enabled, objects present in the existing list but absent in the load list
    /// will be removed from the existing collection.
    /// </summary>
    AllowRemovals = 2,

    /// <summary>
    /// Allows all operations during a patch operation, including both the creation of new objects
    /// and the removal of existing objects.
    /// </summary>
    AllowAll = 3
}

public class Patch
{
    /// <summary>
    /// Gets or sets the threshold for allowable removals in the patch operation as a fraction (0 to 1).
    /// If the ratio of removed items exceeds this threshold, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <value>
    /// A <see cref="double"/> value representing the maximum allowable ratio of removals during the patch.
    /// The default value is 0.75 (75%).
    /// </value>
    public double RemoveThreshold { get; set; } = 0.75;

    private class UpdateableProperty(PropertyInfo propertyInfo, PatchAttribute attribute)
    {
        public PropertyInfo PropertyInfo { get; } = propertyInfo;
        public PatchAttribute Attribute { get; } = attribute;
    }

    private static readonly ConcurrentDictionary<Type, UpdateableProperty[]> Properties = new();

    /// <summary>
    /// Retrieves and caches a collection of properties for a given type that are marked with the <see cref="PatchAttribute"/>.
    /// </summary>
    /// <param name="type">The type for which to index and retrieve the properties with the <see cref="PatchAttribute"/>.</param>
    /// <returns>An array of <c>UpdateableProperty</c> objects representing the indexed properties of the type.</returns>
    private UpdateableProperty[] IndexAttributes(Type type)
    {
        if (Properties.TryGetValue(type, out var properties))
            return properties;

        return Properties[type] = (
            from prop in type.GetProperties()
            let attr = prop.GetCustomAttribute<PatchAttribute>()
            where attr != null
            select new UpdateableProperty(prop, attr)
        ).ToArray();
    }

    /// <summary>
    /// Updates an object by transferring the values from a source object to an existing object.
    /// Only properties marked with the <see cref="PatchAttribute"/> are updated. Handles null
    /// values based on the <c>NullRemovesValue</c> property of the attribute.
    /// </summary>
    /// <typeparam name="T">The type of the objects being updated, which must be a class.</typeparam>
    /// <param name="existing">The existing object to be updated.</param>
    /// <param name="load">The source object containing the updated values.</param>
    /// <returns>Returns <c>true</c> if any changes were made to the existing object, otherwise <c>false</c>.</returns>
    public bool UpdateObject<T>(T existing, T load)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(load);

        var props   = IndexAttributes(typeof(T));
        var changed = false;
        foreach (var prop in props)
        {
            var sourceValue = prop.PropertyInfo.GetValue(load);
            var existingValue = prop.PropertyInfo.GetValue(existing);
            if (sourceValue == existingValue)
                continue;

            if (sourceValue != null)
            {
                prop.PropertyInfo.SetValue(existing, sourceValue);
                changed = true;
            }
            else if (prop.Attribute.NullRemovesValue)
            {
                prop.PropertyInfo.SetValue(existing, null);
                changed = true;
            }
        }

        return changed;
    }

    /// <summary>
    /// Updates an existing object or collection of objects with values from another source. Only the properties tagged with
    /// <see cref="PatchAttribute"/> will be touched. Provides mechanisms to handle object creation, update, and removal.
    /// </summary>
    /// <typeparam name="TItem">The type of the objects being updated, which must be a class and have a parameterless constructor.</typeparam>
    /// <typeparam name="TKey">The type of the object key or id.</typeparam>
    /// <param name="mode">A list of allowed operations (add, remove)</param>
    /// <param name="keySelector">A callback that identifies the key property in the classes.</param>
    /// <param name="existing">The collection of existing objects to be updated. Objects will be added to or removed from this list
    ///     as necessary.</param>
    /// <param name="load">The collection of source objects used to update the existing objects.</param>
    /// <param name="onCreated">An optional action to be invoked when a new object is created and added to the existing collection.</param>
    /// <param name="onChanged">An optional action to be invoked when an existing object is updated with new values.</param>
    /// <returns>The number of changes made, including created, updated, and removed objects.</returns>
    public int Update<TItem, TKey>(PatchMode mode, Func<TItem, TKey> keySelector, List<TItem> existing, List<TItem> load,
        Action<TItem>? onCreated = null, Action<TItem>? onChanged = null)
        where TItem : class, new()
        where TKey : notnull
    {
        var diff    = existing.IntersectCollection(load, keySelector);
        var changes = 0;

        // Remove old objects: Iterate through the list of existing objects and remove all no longer in the load list.
        // We use a HashSet and iterate once through to avoid an O(m*n) situation.
        if (mode.HasFlag(PatchMode.AllowRemovals))
        {
            var removals = diff.Left.ToHashSet();

            var ratio = (double)removals.Count / existing.Count;
            if (ratio > RemoveThreshold)
                throw new InvalidOperationException($"Patch tried to remove {ratio:P} of objects which exceeds the removal threshold ({RemoveThreshold:P}).");

            changes += existing.RemoveAll(x => removals.Contains(x));
        }

        // Add new objects: For every object in load but not in `existing`, create a new object, update the Updateable properties on it,
        // optionally call onCreated and then persist it both to `existing` and the result list.
        if (mode.HasFlag(PatchMode.AllowNewObjects))
        {
            foreach (var loadItem in diff.Right)
            {
                var newItem = new TItem();
                UpdateObject(newItem, loadItem);
                onCreated?.Invoke(newItem);
                existing.Add(newItem);
                changes++;
            }
        }

        // Update all existing objects
        foreach (var (existingItem, loadItem) in diff.Both)
        {
            if (UpdateObject(existingItem, loadItem))
            {
                onChanged?.Invoke(existingItem);
                changes++;
            }
        }

        return changes;
    }
}