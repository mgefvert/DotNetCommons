using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace CommonNetTools
{
    public static class ObjectExtensions
    {
        public static T DeepCopy<T>(this T obj)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);
            }
        }

        public static void SetPropertyValue(this object obj, PropertyInfo property, object value)
        {
            SetPropertyValue(obj, property, value, CultureInfo.CurrentCulture);
        }

        public static void SetPropertyValue(this object obj, PropertyInfo property, object value, CultureInfo culture)
        {
            var propertyType = property.PropertyType;

            // Check to see if we have a nullable type
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && propertyType != typeof(string))
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
            if (ValueIsNull(value) || !value.GetType().DescendantOfOrEqual(propertyType))
            {
                if (propertyType.IsEnum)
                {
                    value = !ValueIsNull(value)
                        ? Enum.Parse(propertyType, Convert.ToString(value))
                        : Enum.GetValues(propertyType).GetValue(0);
                }
                else if (propertyType == typeof(DateTime))
                    value = !ValueIsNull(value) ? DateTime.Parse(value.ToString(), culture) : DateTime.MinValue;
                else if (propertyType == typeof(TimeSpan))
                    value = !ValueIsNull(value) ? TimeSpan.Parse(value.ToString(), culture) : TimeSpan.Zero;
                else if (propertyType == typeof(Guid))
                    value = !ValueIsNull(value) ? Guid.Parse(value.ToString()) : Guid.Empty;
                else
                    value = Convert.ChangeType(value, propertyType);
            }

            property.SetValue(obj, value);
        }

        private static bool ValueIsNull(object value)
        {
            return value == null || (value is string && string.IsNullOrEmpty((string)value));
        }
    }
}
