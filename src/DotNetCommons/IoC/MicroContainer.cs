using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetCommons.Collections;

namespace DotNetCommons.IoC
{
    public enum CreationMode
    {
        Create,
        Singleton
    }

    public class MicroContainer : IDisposable
    {
        public delegate object Creator(MicroContainer container);

        protected Dictionary<string, object> Configuration { get; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        protected readonly Dictionary<Type, Entry> Map = new Dictionary<Type, Entry>();

        protected class ConstructorEntry
        {
            public ConstructorInfo Constructor;
            public ParameterInfo[] Parameters;
            public int ParameterCount;
            public bool Resolved;

            public ConstructorEntry(ConstructorInfo info)
            {
                Constructor = info;
                Parameters = info.GetParameters();
                ParameterCount = Parameters.Length;
            }
        }

        protected class Entry
        {
            public Creator Creator;
            public PropertyInfo[] ImplementationProperties;
            public Type ImplementationType;
            public object Instance;
            public bool InstanceOwned;
            public CreationMode Mode;
            public ConstructorEntry ResolvedConstructor;
            public object ResolvedParameters { get; set; }

            public Entry Copy()
            {
                return new Entry
                {
                    Creator = Creator,
                    ImplementationProperties = ImplementationProperties,
                    ImplementationType = ImplementationType,
                    Instance = InstanceOwned ? null : Instance,
                    Mode = Mode,
                    ResolvedConstructor = ResolvedConstructor,
                    ResolvedParameters = ResolvedParameters
                };
            }
        }

        protected void ClearResolved()
        {
            foreach (var entry in Map.Values)
            {
                entry.ResolvedConstructor = null;
                entry.ResolvedParameters = null;
            }
        }

        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        public object Create(Type type)
        {
            var entry = Map.GetOrDefault(type);
            if (entry == null)
                return CreateNew(new Entry
                {
                    ImplementationType = type,
                    Mode = CreationMode.Create
                });

            if (entry.Instance != null)
                return entry.Instance;

            var result = entry.Creator != null ? entry.Creator(this) : CreateNew(entry);
            if (entry.Mode == CreationMode.Singleton)
            {
                entry.Instance = result;
                entry.InstanceOwned = true;
            }

            return result;
        }

        protected object CreateNew(Entry entry)
        {
            if (entry.ResolvedConstructor == null)
            {
                entry.ResolvedConstructor = ResolveConstructor(entry.ImplementationType);
                entry.ImplementationProperties = entry.ImplementationType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => Map.ContainsKey(p.PropertyType))
                    .ToArray();
            }

            var c = entry.ResolvedConstructor;
            var args = new object[c.ParameterCount];
            for (var i = 0; i < args.Length; i++)
            {
                if (Map.ContainsKey(c.Parameters[i].ParameterType))
                    args[i] = Create(c.Parameters[i].ParameterType);
            }

            var result = c.Constructor.Invoke(args);
            foreach (var property in entry.ImplementationProperties)
                if (property.GetValue(result) == null)
                    property.SetValue(result, Create(property.PropertyType));

            return result;
        }

        public void Dispose()
        {
            foreach (var entry in Map.Values.Where(e => e.Instance != null && e.InstanceOwned))
            {
                if (entry.Instance is IDisposable disposable)
                    disposable.Dispose();

                entry.Instance = null;
            }
        }

        public void Clear()
        {
            Dispose();
            Map.Clear();
        }

        public void Clear<T>()
        {
            Clear(typeof(T));
        }

        protected void Clear(Type type)
        {
            var entry = Map.GetOrDefault(type);
            if (entry == null)
                return;

            if (entry.Instance is IDisposable disposable)
                disposable.Dispose();

            Map.Remove(type);
        }

        public MicroContainer Local()
        {
            var result = new MicroContainer();

            foreach (var x in Configuration)
                result.Configuration[x.Key] = x.Value;

            foreach (var x in Map)
                result.Map[x.Key] = x.Value.Copy();

            return result;
        }

        protected Entry NewEntry(Type type, CreationMode mode)
        {
            Clear(type);
            ClearResolved();

            var result = new Entry { ImplementationType = type, Mode = mode };
            Map.Add(type, result);
            return result;
        }

        protected ConstructorEntry ResolveConstructor(Type type)
        {
            var constructors = type.GetConstructors()
                .Select(c => new ConstructorEntry(c))
                .Where(c => c.ParameterCount == 0 || c.Parameters.All(p => p.ParameterType != type && (Map.ContainsKey(p.ParameterType) || p.HasDefaultValue)))
                .OrderByDescending(x => x.ParameterCount)
                .ToList();

            return constructors.FirstOrDefault() ?? throw new ArgumentException($"Unable to instantiate type {type.Name}, no suitable constructor found");
        }

        public T Get<T>(string configKey)
        {
            return Configuration.TryGetValue(configKey, out var value) ? (T) value : default(T);
        }

        public MicroContainer Register<TInterface, TImplementation>(CreationMode mode = CreationMode.Create)
        {
            NewEntry(typeof(TInterface), mode).ImplementationType = typeof(TImplementation);
            return this;
        }

        public MicroContainer Register<TInterface>(Creator creator, CreationMode mode = CreationMode.Create)
        {
            NewEntry(typeof(TInterface), mode).Creator = creator;
            return this;
        }

        public MicroContainer Register<TInterface>(TInterface instance)
        {
            NewEntry(typeof(TInterface), CreationMode.Singleton).Instance = instance;
            return this;
        }

        public T Require<T>(string configKey)
        {
            return Configuration.TryGetValue(configKey, out var value)
                ? (T) value
                : throw new InvalidOperationException($"Key {configKey} does not exist.");
        }

        public MicroContainer Set(string configKey, object value)
        {
            Configuration[configKey] = value;
            return this;
        }
    }
}
