using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons
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

        public static dynamic DeepCopy(dynamic obj)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;

                return formatter.Deserialize(stream);
            }
        }

        public static void Copy(object source, object target)
        {
            if (source == null || target == null)
                return;

            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var sourceProps = source.GetType().GetProperties(flags);
            var targetProps = target.GetType().GetProperties(flags);

            foreach (var targetProp in targetProps.Where(x => x.CanWrite))
            {
                var sourceProp = sourceProps.FirstOrDefault(p => p.Name == targetProp.Name && p.PropertyType == targetProp.PropertyType);
                if (sourceProp != null)
                {
                    var value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, value);
                }
            }
        }

        public static T Copy<T>(object source) where T : class, new()
        {
            if (source == null)
                return null;

            var result = new T();
            Copy(source, result);
            return result;
        }

        public static void SetPropertyValue(this object obj, PropertyInfo property, object value)
        {
            SetPropertyValue(obj, property, value, CultureInfo.CurrentCulture);
        }

        public static void SetPropertyValue(this object obj, PropertyInfo property, object value, CultureInfo culture)
        {
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
            if (ValueIsNull(value) || !value.GetType().DescendantOfOrEqual(propertyType))
            {
                if (propertyType.IsEnum)
                {
                    if (ValueIsNull(value))
                        value = Enum.GetValues(propertyType).GetValue(0);
                    else
                    {
                        var str = Convert.ToString(value).Replace("-", "");
                        value = Enum.Parse(propertyType, str, true);
                    }
                }
                else if (propertyType == typeof(DateTime))
                    value = !ValueIsNull(value) ? DateTime.Parse(value.ToString(), culture) : DateTime.MinValue;
                else if (propertyType == typeof(TimeSpan))
                    value = !ValueIsNull(value) ? TimeSpan.Parse(value.ToString(), culture) : TimeSpan.Zero;
                else if (propertyType == typeof(Guid))
                    value = !ValueIsNull(value) ? Guid.Parse(value.ToString()) : Guid.Empty;
                else
                    value = Convert.ChangeType(value, propertyType, culture);
            }

            property.SetValue(obj, value);
        }

        private static bool ValueIsNull(object value)
        {
            return value == null || (value is string && string.IsNullOrEmpty((string)value));
        }
    }
}
