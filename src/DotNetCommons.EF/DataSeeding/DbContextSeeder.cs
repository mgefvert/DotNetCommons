using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.EF.DataSeeding;

/// Provides functionality to seed data into a DbSet of a DbContext in Entity Framework.
/// Ensures records are created or updated in the database based on the provided criteria.
/// Accessed by using the DbSet.Seed(s => ...) construct.
public class DbContextSeeder<T>
    where T : class, new()
{
    private readonly DbSet<T> _dbSet;
    private readonly Patch _patch = new(allowAnyField: true);

    internal DbContextSeeder(DbSet<T> dbSet)
    {
        _dbSet = dbSet;
    }

    /// <summary>
    /// Ensures that the records defined in the seeding data are created in the database, while preventing the creation of duplicate
    /// entries based on a specified match field. Updates existing records or adds new ones based on the provided data.
    /// </summary>
    /// <param name="matchField">An expression specifying the property to match records in the database and the seeding data.</param>
    /// <param name="records">A list of records to be created or updated in the database.</param>
    /// <returns>The number of records added or updated in the database.</returns>
    public int EnsureCreated<TKey>(Expression<Func<T, TKey>> matchField, List<T> records)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(matchField);
        if (records.Count == 0)
            return 0;

        var keyGetter  = matchField.Compile();
        var recordKeys = records.Select(keyGetter).ToList();
        var multiples = recordKeys.GroupBy(x => x).Where(g => g.Count() > 1).ToArray();
        if (multiples.Any())
            throw new ArgumentException($"Duplicate keys found in seeding data: {string.Join(", ", multiples.Select(g => g.Key))}");

        var existingRecords = _dbSet.LoadByPropertySelector(matchField, recordKeys);

        return _patch.Update(PatchMode.AllowNewObjects, keyGetter, existingRecords, records,
            created => _dbSet.Add(created));
    }

    /// <summary>
    /// Ensures that the enumeration values are represented as records in the database. Adds new records or updates existing ones based
    /// on the specified fields for value, name, and optional description.
    /// </summary>
    /// <param name="valueField">An expression specifying the property that corresponds to the enumeration value.</param>
    /// <param name="nameField">An expression specifying the property that corresponds to the enumeration name.</param>
    /// <param name="descriptionField">An optional expression specifying the property that corresponds to an enumeration description.</param>
    /// <returns>The number of records added or updated in the database.</returns>
    public int EnsureEnumsCreated<TEnum>(
        Expression<Func<T, object>> valueField,
        Expression<Func<T, object>> nameField,
        Expression<Func<T, object?>>? descriptionField = null
    )
        where TEnum : Enum
    {
        var records = EnumToRecords(typeof(TEnum), valueField, nameField, descriptionField);
        return EnsureCreated(valueField, records);
    }

    /// <summary>
    /// Converts the members of an enum into a list of records that can be used to seed a DbSet in Entity Framework.
    /// Each enum member is transformed into a record containing specified value, name, and optional description fields.
    /// </summary>
    /// <param name="valueField">An expression specifying the property in the target class to map to the enum's underlying value.</param>
    /// <param name="nameField">An expression specifying the property in the target class to map to the enum's name.</param>
    /// <param name="descriptionField"> An optional expression specifying the property in the target class to map to the enum's description,
    ///     or null if no description is required. </param>
    /// <returns>A list of records created from the members of the specified enum.</returns>
    public List<T> EnumToRecords<TEnum>(
        Expression<Func<T, object>> valueField,
        Expression<Func<T, object>> nameField,
        Expression<Func<T, object?>>? descriptionField = null
    )
        where TEnum : Enum
    {
        return EnumToRecords(typeof(TEnum), valueField, nameField, descriptionField);
    }

    /// <summary>
    /// Converts the members of an enum into a list of records that can be used to seed a DbSet in Entity Framework.
    /// Each enum member is transformed into a record containing specified value, name, and optional description fields.
    /// </summary>
    /// <param name="enumType">The Enum to convert into records.</param>
    /// <param name="valueField">An expression specifying the property in the target class to map to the enum's underlying value.</param>
    /// <param name="nameField">An expression specifying the property in the target class to map to the enum's name.</param>
    /// <param name="descriptionField"> An optional expression specifying the property in the target class to map to the enum's description,
    ///     or null if no description is required. </param>
    /// <returns>A list of records created from the members of the specified enum.</returns>
    public List<T> EnumToRecords(
        Type enumType,
        Expression<Func<T, object>> valueField,
        Expression<Func<T, object>> nameField,
        Expression<Func<T, object?>>? descriptionField = null
    )
    {
        var valueProperty       = GetProperty(valueField!);
        var nameProperty        = GetProperty(nameField!);
        var descriptionProperty = descriptionField != null ? GetProperty(descriptionField) : null;

        var result = new List<T>();
        foreach (var name in Enum.GetNames(enumType))
        {
            var field = enumType.GetField(name);
            if (field == null)
                continue;

            var value = field.GetValue(null);
            var description = descriptionProperty != null
                ? field.GetCustomAttribute<DescriptionAttribute>()?.Description
                : null;

            var newRecord = new T();
            valueProperty?.SetPropertyValue(newRecord, value);
            nameProperty?.SetPropertyValue(newRecord, name);
            descriptionProperty?.SetPropertyValue(newRecord, description);

            result.Add(newRecord);
        }

        return result;
    }

    private static PropertyInfo GetProperty(Expression<Func<T, object?>> expression)
    {
        var body = expression.Body;

        // Handle potential unary conversion (e.g., value types boxed to object)
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            body = unary.Operand;

        // Extract member expression and return PropertyInfo if the member is a property
        var propertyInfo = body is MemberExpression memberExpression
            ? memberExpression.Member as PropertyInfo
            : null;

        return propertyInfo ?? throw new InvalidOperationException("Property not found.");
    }
}