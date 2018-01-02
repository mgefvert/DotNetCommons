using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using DotNetCommons.Text;

namespace DotNetCommons
{
    public static class SysConfig
    {
        // Public methods

        public static T Create<T>(string filename) where T : class, new()
        {
            var result = new T();
            LoadIntoObject(filename, result);
            return result;
        }

        public static void LoadIntoObject(string filename, object result)
        {
            var config = LoadConfigIntoDictionary(filename);
            foreach (var item in config)
                MapValue(item.Key, item.Key, item.Value, result, result.GetType(), false);
        }

        public static void LoadIntoClass(string filename, Type staticClass)
        {
            var config = LoadConfigIntoDictionary(filename);
            foreach (var item in config)
                MapValue(item.Key, item.Key, item.Value, null, staticClass, true);
        }

        public static Dictionary<string, string> LoadConfigIntoDictionary(string filename)
        {
            var result = new Dictionary<string, string>();
            var basename = Path.GetFileNameWithoutExtension(filename);
            var ext = Path.GetExtension(filename);

            var file = FindSystemConfigFile(basename + ext);
            if (file != null)
                ParseFile(result, file);

            file = FindSystemConfigFile(basename + "." + Environment.MachineName + ext);
            if (file != null)
                ParseFile(result, file);

            return result;
        }

        // Private methods

        private static PropertyInfo FindProperty(string originalKey, string propertyName, Type type, BindingFlags flags)
        {
            var property = type
                .GetProperties(flags)
                .FirstOrDefault(p => propertyName.Like(p.Name));

            if (property == null)
                throw new KeyNotFoundException($"Unable to map configuration value '{originalKey}' to property.");

            return property;
        }

        private static FileInfo FindSystemConfigFile(string name)
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                var file = dir.EnumerateFiles(name).FirstOrDefault();
                if (file != null)
                    return file;

                dir = dir.Parent;
            }

            return null;
        }

        private static void MapValue(string originalKey, string key, string value, object result, Type resultType, bool isStatic)
        {
            var flags = isStatic
                ? BindingFlags.Static | BindingFlags.Public
                : BindingFlags.Instance | BindingFlags.Public;

            if (key.Contains("."))
            {
                // Dig deeper into object
                var sections = key.Split(new[] { '.' }, 2);
                var property = FindProperty(originalKey, sections[0], resultType, flags);
                var child = property.GetValue(result);
                if (child == null)
                    throw new KeyNotFoundException($"Unable to map configuration value '{originalKey}' because the property for '{sections[0]}' is null.");

                MapValue(originalKey, sections[1], value, child, child.GetType(), false);
            }
            else
            {
                // Set the property to the value
                var property = FindProperty(originalKey, key, resultType, flags);
                result.SetPropertyValue(property, value, CultureInfo.InvariantCulture);
            }
        }

        private static void ParseFile(Dictionary<string, string> values, FileInfo file)
        {
            using (var fs = file.OpenRead())
            {
                var reader = XmlReader.Create(fs);
                var path = new Stack<string>();
                string root = null;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (!reader.IsEmptyElement)
                            {
                                if (root == null)
                                    root = reader.Name;
                                else
                                    path.Push(reader.Name.Replace("-", ""));
                            }
                            break;

                        case XmlNodeType.Text:
                            values[string.Join(".", path.Reverse())] = reader.Value.Replace("~/", file.DirectoryName + Path.DirectorySeparatorChar);
                            break;

                        case XmlNodeType.EndElement:
                            if (path.Any())
                                path.Pop();
                            break;
                    }
                }
            }
        }
    }
}
