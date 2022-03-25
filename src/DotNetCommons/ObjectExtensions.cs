using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DotNetCommons.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class ObjectExtensions
{
    /// <summary>
    /// Set the value of a property to a specific value, handling conversions from a number
    /// of recognized formats in the process.
    /// </summary>
    public static void SetPropertyValue(this object obj, PropertyInfo property, object? value)
    {
        SetPropertyValue(obj, property, value, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Set the value of a property to a specific value, handling conversions from a number
    /// of recognized formats in the process.
    /// </summary>
    public static void SetPropertyValue(this object obj, PropertyInfo property, object? value, CultureInfo culture)
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