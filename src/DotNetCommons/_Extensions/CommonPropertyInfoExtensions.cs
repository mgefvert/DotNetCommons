using System.Globalization;
using System.Reflection;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonPropertyInfoExtensions
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
        property.SetValue(obj, property.ConvertPropertyValue(value, culture));
    }

    /// <summary>
    /// Sets the value of a property on the specified object, converting the value to match the property's type
    /// if necessary, using the given culture and optional exact format for supported date/time types.
    /// </summary>
    /// <param name="property">The PropertyInfo instance representing the property to set.</param>
    /// <param name="obj">The target object on which to set the property value.</param>
    /// <param name="value">The new value to assign to the property, which may be converted to the appropriate type.</param>
    /// <param name="culture">The CultureInfo instance used for type conversion if needed.</param>
    /// <param name="format">Optional exact format for DateTime, DateTimeOffset, DateOnly, TimeOnly, and TimeSpan parsing.</param>
    public static void SetPropertyValue(this PropertyInfo property, object obj, object? value, CultureInfo culture, string? format)
    {
        property.SetValue(obj, property.ConvertPropertyValue(value, culture, format));
    }

    /// <summary>
    /// Converts a value to the specified property's type without setting the property. This is useful for callers
    /// that cache their own setters but want the same conversion rules as SetPropertyValue.
    /// </summary>
    public static object? ConvertPropertyValue(this PropertyInfo property, object? value)
    {
        return property.ConvertPropertyValue(value, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Converts a value to the specified property's type without setting the property, using the given culture.
    /// </summary>
    public static object? ConvertPropertyValue(this PropertyInfo property, object? value, CultureInfo culture)
    {
        return ConvertPropertyValue(property, value, culture, null);
    }

    /// <summary>
    /// Converts a value to the specified property's type without setting the property, using the given culture
    /// and optional exact format for supported date/time types.
    /// </summary>
    public static object? ConvertPropertyValue(this PropertyInfo property, object? value, CultureInfo culture, string? format)
    {
        return ConvertValue(value, property.PropertyType, culture, format);
    }

    /// <summary>
    /// Converts a value to the requested target type using the same conversion rules as SetPropertyValue.
    /// </summary>
    public static object? ConvertValue(this Type targetType, object? value, CultureInfo culture, string? format = null)
    {
        return ConvertValue(value, targetType, culture, format);
    }

    private static object? ConvertValue(object? value, Type targetType, CultureInfo culture, string? format)
    {
        var valueIsNull = ValueIsNull(value);
        var propertyType = targetType;

        // Check to see if we have a nullable type
        if (propertyType.IsNullable())
        {
            // Null value, set property quite simply to null
            if (valueIsNull)
                return null;

            // Force the property to its underlying type - e.g. Nullable<DateTime> to DateTime
            propertyType = propertyType.GetGenericArguments().Single();
        }

        // Type conversion required?
        if (valueIsNull || !value!.GetType().DescendantOfOrEqual(propertyType))
        {
            if (propertyType.IsEnum)
            {
                if (valueIsNull)
                    value = Enum.GetValues(propertyType).GetValue(0);
                else
                {
                    var str = System.Convert.ToString(value, culture)!.Replace("-", "");
                    value = Enum.Parse(propertyType, str, true);
                }
            }
            else if (propertyType == typeof(DateTimeOffset))
                value = !valueIsNull ? ParseDateTimeOffset(value!, culture, format) : DateTimeOffset.MinValue;
            else if (propertyType == typeof(DateTime))
                value = !valueIsNull ? ParseDateTime(value!, culture, format) : DateTime.MinValue;
            else if (propertyType == typeof(DateOnly))
            {
                if (value is DateTime dt)
                    value = DateOnly.FromDateTime(dt);
                else if (!valueIsNull)
                    value = ParseDateOnly(value!, culture, format);
                else
                    value = DateOnly.MinValue;
            }
            else if (propertyType == typeof(TimeOnly))
                value = !valueIsNull ? ParseTimeOnly(value!, culture, format) : TimeOnly.MinValue;
            else if (propertyType == typeof(TimeSpan))
                value = !valueIsNull ? ParseTimeSpan(value!, culture, format) : TimeSpan.Zero;
            else if (propertyType == typeof(Guid))
                value = !valueIsNull ? Guid.Parse(value!.ToString()!) : Guid.Empty;
            else if (propertyType == typeof(Uri))
                value = !valueIsNull ? new Uri(value!.ToString()!) : null;
            else if (propertyType == typeof(bool))
                value = !valueIsNull && value!.ToString()!.ParseBoolean(false);
            else if (!valueIsNull && propertyType.IsInterface && value!.GetType().IsAssignableTo(propertyType))
            {
                // If the value already implements the interface requested by the target property, do nothing
            }
            else if (valueIsNull && propertyType.IsValueType)
                value = Activator.CreateInstance(propertyType);
            else
                value = System.Convert.ChangeType(value, propertyType, culture);
        }

        return value;
    }

    private static bool ValueIsNull(object? value)
    {
        return value == null || value is string s && string.IsNullOrEmpty(s);
    }

    private static DateTimeOffset ParseDateTimeOffset(object value, CultureInfo culture, string? format)
    {
        var s = value.ToString()!;
        return format.IsSet() ? DateTimeOffset.ParseExact(s, format, culture) : DateTimeOffset.Parse(s, culture);
    }

    private static DateTime ParseDateTime(object value, CultureInfo culture, string? format)
    {
        var s = value.ToString()!;
        return format.IsSet() ? DateTime.ParseExact(s, format, culture) : DateTime.Parse(s, culture);
    }

    private static DateOnly ParseDateOnly(object value, CultureInfo culture, string? format)
    {
        var s = value.ToString()!;
        return format.IsSet() ? DateOnly.ParseExact(s, format, culture) : DateOnly.Parse(s, culture);
    }

    private static TimeOnly ParseTimeOnly(object value, CultureInfo culture, string? format)
    {
        var s = value.ToString()!;
        return format.IsSet() ? TimeOnly.ParseExact(s, format, culture) : TimeOnly.Parse(s, culture);
    }

    private static TimeSpan ParseTimeSpan(object value, CultureInfo culture, string? format)
    {
        var s = value.ToString()!;
        return format.IsSet() ? TimeSpan.ParseExact(s, format, culture) : TimeSpan.Parse(s, culture);
    }
}
