using System.Globalization;
using System.Reflection;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonObjectExtensions
{
    /// <summary>
    /// Determines whether the specified property of an object has its default value,
    /// based on the property's type.
    /// </summary>
    /// <param name="propertyInfo">The PropertyInfo instance representing the property to check.</param>
    /// <param name="obj">The object containing the property to evaluate.</param>
    /// <returns>True if the property's current value matches the default value for its type; otherwise, false.</returns>
    public static bool HasDefaultValue(this PropertyInfo propertyInfo, object obj)
    {
        var defaultValue = propertyInfo.PropertyType.IsValueType
            ? Activator.CreateInstance(propertyInfo.PropertyType)
            : null;

        return propertyInfo.GetValue(obj) == defaultValue;
    }

    /// <summary>
    /// Set the value of a property to a specific value, handling conversions from a number
    /// of recognized formats in the process.
    /// </summary>
    public static void SetPropertyValue(this PropertyInfo property, object obj, object? value)
    {
        SetPropertyValue(property, obj, value, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Sets the value of a property on the specified object, converting the value to match the property's type
    /// if necessary, using the given culture for type conversion.
    /// </summary>
    /// <param name="property">The PropertyInfo instance representing the property to set.</param>
    /// <param name="obj">The target object on which to set the property value.</param>
    /// <param name="value">The new value to assign to the property, which may be converted to the appropriate type.</param>
    /// <param name="culture">The CultureInfo instance used for type conversion if needed.</param>
    public static void SetPropertyValue(this PropertyInfo property, object obj, object? value, CultureInfo culture)
    {
        bool ValueIsNull(object? v) => v == null || v is string s && string.IsNullOrEmpty(s);

        var propertyType = property.PropertyType;

        // Check to see if we have a nullable type
        if (propertyType.IsNullable())
        {
            // Null value, set property quite simply to null
            if (ValueIsNull(value))
            {
                property.SetValue(obj, null);
                return;
            }

            // Force the property to its underlying type - e.g. Nullable<DateTime> to DateTime
            propertyType = propertyType.GetGenericArguments().Single();
        }

        // Type conversion required?
        if (ValueIsNull(value) || !value!.GetType().DescendantOfOrEqual(propertyType))
        {
            if (propertyType.IsEnum)
            {
                if (ValueIsNull(value))
                    value = Enum.GetValues(propertyType).GetValue(0);
                else
                {
                    var str = Convert.ToString(value)!.Replace("-", "");
                    value = Enum.Parse(propertyType, str, true);
                }
            }
            else if (propertyType == typeof(DateTimeOffset))
                value = !ValueIsNull(value) ? DateTimeOffset.Parse(value!.ToString()!, culture) : DateTimeOffset.MinValue;
            else if (propertyType == typeof(DateTime))
                value = !ValueIsNull(value) ? DateTime.Parse(value!.ToString()!, culture) : DateTime.MinValue;
            else if (propertyType == typeof(TimeSpan))
                value = !ValueIsNull(value) ? TimeSpan.Parse(value!.ToString()!, culture) : TimeSpan.Zero;
            else if (propertyType == typeof(Guid))
                value = !ValueIsNull(value) ? Guid.Parse(value!.ToString()!) : Guid.Empty;
            else if (propertyType == typeof(Uri))
                value = !ValueIsNull(value) ? new Uri(value!.ToString()!) : null;
            else if (propertyType == typeof(bool))
                value = !ValueIsNull(value) && value!.ToString()!.ParseBoolean(false);
            else
                value = Convert.ChangeType(value, propertyType, culture);
        }

        property.SetValue(obj, value);
    }
}