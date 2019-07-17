using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetCommons.Collections;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IoC
{
    public enum CreationMode
    {
        /// <summary>
        /// Create a new object every time it's requested.
        /// </summary>
        Create,
        /// <summary>
        /// Keep a single object on hand and delegate references to it.
        /// </summary>
        Singleton
    }

    /// <summary>
    /// A microscopic inversion of control container.
    /// </summary>
    public class MicroContainer : IDisposable
    {
        private static MicroContainer _default;

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

        /// <summary>
        /// Acquire a reference to a global, default instance.
        /// </summary>
        /// <returns>The default singleton instance.</returns>
        public static MicroContainer DefaultInstance()
        {
            return _default ?? (_default = new MicroContainer());
        }

        /// <summary>
        /// Acquire an object of type T, as long as it's been registered in the container - either a new object
        /// or a singleton object, depending on the registration.
        /// </summary>
        /// <typeparam name="T">Type to create.</typeparam>
        /// <returns>An object of type T.</returns>
        public T Acquire<T>()
        {
            return (T)Acquire(typeof(T));
        }

        /// <summary>
        /// Acquire an object of a specific type, as long as it's been registered in the container - either a new object
        /// or a singleton object, depending on the registration.
        /// </summary>
        /// <param name="type">Type to create.</param>
        /// <returns>An object of the given type.</returns>
        public object Acquire(Type type)
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

        /// <summary>
        /// Clear the entire container from both mappings and singletons.
        /// </summary>
        public void Clear()
        {
            Dispose();
            Map.Clear();
        }

        /// <summary>
        /// Clear the container from a specific type, also removing the mapping.
        /// </summary>
        /// <param name="type">Type to clear</param>
        protected void Clear(Type type)
        {
            var entry = Map.GetOrDefault(type);
            if (entry == null)
                return;

            if (entry.Instance is IDisposable disposable)
                disposable.Dispose();

            Map.Remove(type);
        }

        /// <summary>
        /// Clear the container from a specific type, also removing the mapping.
        /// </summary>
        /// <typeparam name="T">Type to clear</typeparam>
        public void Clear<T>()
        {
            Clear(typeof(T));
        }

        protected void ClearResolved()
        {
            foreach (var entry in Map.Values)
            {
                entry.ResolvedConstructor = null;
                entry.ResolvedParameters = null;
            }
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
                    args[i] = Acquire(c.Parameters[i].ParameterType);
            }

            var result = c.Constructor.Invoke(args);
            foreach (var property in entry.ImplementationProperties)
                if (property.GetValue(result) == null)
                    property.SetValue(result, Acquire(property.PropertyType));

            return result;
        }

        /// <summary>
        /// Dispose of all created objects (singletons).
        /// </summary>
        public void Dispose()
        {
            foreach (var entry in Map.Values.Where(e => e.Instance != null && e.InstanceOwned))
            {
                if (entry.Instance is IDisposable disposable)
                    disposable.Dispose();

                entry.Instance = null;
            }
        }

        /// <summary>
        /// Get a configuration value.
        /// </summary>
        /// <typeparam name="T">Type to convert to.</typeparam>
        /// <param name="configKey">Configuration key to look for.</param>
        /// <returns>The configuration value, or default(T) if not found.</returns>
        public T GetConfigValue<T>(string configKey)
        {
            return Configuration.TryGetValue(configKey, out var value) ? (T) value : default;
        }

        /// <summary>
        /// Create a copy of the container for local purposes.
        /// </summary>
        /// <returns>A newly created container (with only mappings, no duplicated singletons).</returns>
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

        /// <summary>
        /// Register an object as an implementation of an interface.
        /// </summary>
        /// <typeparam name="TInterface">Interface to register.</typeparam>
        /// <typeparam name="TImplementation">Implementation to provide.</typeparam>
        /// <param name="mode">Creation mode (singleton or new object)</param>
        /// <returns>A reference to the same container.</returns>
        public MicroContainer Register<TInterface, TImplementation>(CreationMode mode)
        {
            NewEntry(typeof(TInterface), mode).ImplementationType = typeof(TImplementation);
            return this;
        }

        /// <summary>
        /// Register an creation method an implementation provider for an interface.
        /// </summary>
        /// <typeparam name="TInterface">Interface to register.</typeparam>
        /// <param name="creator">Creation callback.</param>
        /// <param name="mode">Creation mode (singleton or new object)</param>
        /// <returns>A reference to the same container.</returns>
        public MicroContainer Register<TInterface>(Creator creator, CreationMode mode)
        {
            NewEntry(typeof(TInterface), mode).Creator = creator;
            return this;
        }

        /// <summary>
        /// Register an object as an interface singleton.
        /// </summary>
        /// <typeparam name="TInterface">Object to register.</typeparam>
        /// <returns>A reference to the same container.</returns>
        public MicroContainer Register<TInterface>(TInterface instance)
        {
            NewEntry(typeof(TInterface), CreationMode.Singleton).Instance = instance;
            return this;
        }

        /// <summary>
        /// Get a configuration value, throwing an exception if it doesn't exist.
        /// </summary>
        /// <typeparam name="T">Type to convert to.</typeparam>
        /// <param name="configKey">Configuration key to look for.</param>
        /// <returns>The configuration value.</returns>
        public T RequireConfig<T>(string configKey)
        {
            return Configuration.TryGetValue(configKey, out var value)
                ? (T) value
                : throw new InvalidOperationException($"Key {configKey} does not exist.");
        }

        /// <summary>
        /// Set a configuration key to a value.
        /// </summary>
        /// <param name="configKey">Configuration key to set.</param>
        /// <param name="value">Associated value.</param>
        /// <returns>A reference to the same container.</returns>
        public MicroContainer SetConfig(string configKey, object value)
        {
            Configuration[configKey] = value;
            return this;
        }
    }
}
