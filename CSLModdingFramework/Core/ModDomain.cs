using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSLModdingFramework.Logging;
using UnityEngine;

namespace CSLModdingFramework.Core;

public sealed class ModDomain {
    internal static readonly Dictionary<Type, ModDomain> DomainsByManagerType = new();
    private static readonly ILog Logger = LogManager.GetLogger();

    internal readonly Dictionary<Type, SystemBase> _systemLookup = new();

    static ModDomain() => Logger.Info($"ModDomain initialized, version: {FrameworkManager.CurrentVersion}");

    public ModDomain(string name) => Name = name;

    internal static event Action<ModDomain, SystemBase> SystemCreated;
    internal static event Action<ModDomain, SystemBase> SystemDestroyed;
    public static RuntimePlatform CurrentPlatform { get; } = Application.platform;
    
    public AssemblyName CurrentAssemblyName { get; private set; }
    public ModSystemBase ModSystem { get; private set; }
    public string Name { get; }

    internal static ModDomain GetOrCreateDomain<T>() where T : ModSystemBase, new() {
        var type = typeof(T);
        if (DomainsByManagerType.TryGetValue(type, out var existing)) {
            return existing;
        }

        var domain = new ModDomain(type.Namespace);
        domain.CreateModSystem<T>();

        Logger.Info($"Domain created for {type.AssemblyQualifiedName}");

        return domain;
    }

    internal static IEnumerable<ModDomain> ListAllDomains() => DomainsByManagerType.Values;

    internal static bool RemoveDomain<T>() where T : ModSystemBase {
        var type = typeof(T);
        if (!DomainsByManagerType.TryGetValue(type, out var domain)) return false;

        domain.DestroyAllSystems();
        DomainsByManagerType.Remove(type);

        Logger.Info($"Domain removed for {type.AssemblyQualifiedName}");
        return true;
    }

    internal static bool TryGetDomain<T>(out ModDomain domain) where T : ModSystemBase => DomainsByManagerType.TryGetValue(typeof(T), out domain);

    public T AddSystem<T>(T system) where T : SystemBase {
        if (system == null)
            throw new ArgumentNullException(nameof(system));

        var type = system.GetType();

        _systemLookup[type] = system;
        CreateSystemInternal(system);

        return system;
    }

    public SystemBase CreateSystem(Type type) {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (!typeof(SystemBase).IsAssignableFrom(type))
            throw new ArgumentException($"Type {type.Name} does not inherit SystemBase");

        try {
            var instance = (SystemBase)Activator.CreateInstance(type, true);

            _systemLookup[type] = instance;
            CreateSystemInternal(instance);
            return instance;
        }
        catch (Exception ex) {
            Logger.Error(ex, $"Error creating system {type.Name}");
            return null;
        }
    }

    public T CreateSystem<T>() where T : SystemBase, new() {
        var type = typeof(T);
        var instance = new T();

        _systemLookup[type] = instance;
        CreateSystemInternal(instance);
        return instance;
    }

    public void DestroySystem(SystemBase system) {
        if (system == null)
            return;

        var type = system.GetType();
        try {
            SystemDestroyed?.Invoke(this, system);
        }
        catch (Exception ex) {
            Logger.Error(ex, $"SystemDestroyed event handler threw an exception for {type.Name}");
        }

        try {
            system.DestroyInstance();
        }
        catch (Exception ex) {
            Logger.Error(ex, $"Error destroying system instance {type.Name}");
        }

        _systemLookup.Remove(type);
    }

    public IEnumerable<T> FilterSystems<T>(Func<T, bool> predicate) where T : SystemBase => _systemLookup.Values.OfType<T>().Where(predicate);

    public T GetExistingSystem<T>() where T : SystemBase {
        if (_systemLookup.TryGetValue(typeof(T), out var system))
            return (T)system;

        return null;
    }

    public SystemBase GetExistingSystem(Type type) {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        _systemLookup.TryGetValue(type, out var system);
        return system;
    }

    public T GetOrCreateSystem<T>() where T : SystemBase, new() {
        if (_systemLookup.TryGetValue(typeof(T), out var system)) return (T)system;

        var type = typeof(T);
        var instance = new T();
        try {
            _systemLookup[type] = instance;
            CreateSystemInternal(instance);
            return instance;
        }
        catch (Exception ex) {
            Logger.Error(ex, $"Error creating system {type.Name}");
            return null;
        }
    }

    public SystemBase GetOrCreateSystem(Type type) {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (!typeof(SystemBase).IsAssignableFrom(type))
            throw new ArgumentException($"Type {type.Name} does not inherit SystemBase");

        if (_systemLookup.TryGetValue(type, out var existing))
            return existing;

        try {
            var instance = (SystemBase)Activator.CreateInstance(type, true);

            _systemLookup[type] = instance;
            CreateSystemInternal(instance);
            return instance;
        }
        catch (Exception ex) {
            Logger.Error(ex, $"Error creating system {type.Name}");
            return null;
        }
    }

    public bool HasSystem<T>() => _systemLookup.ContainsKey(typeof(T));

    public IEnumerable<Type> ListSystemsTypes() => _systemLookup.Keys;

    internal T CreateModSystem<T>() where T : ModSystemBase, new() {
        var type = typeof(T);

        if (DomainsByManagerType.ContainsKey(type)) {
            throw new InvalidOperationException($"Domain for manager type {type.FullName} already exists.");
        }

        DomainsByManagerType[type] = this;

        var instance = new T();
        try {
            _systemLookup[type] = instance;
            ModSystem = instance;
            CurrentAssemblyName = instance.CurrentAssemblyName;
            CreateSystemInternal(instance);
        }
        catch (Exception ex) {
            Logger.Error(ex, $"Error creating system {type.Name}");
            return null;
        }

        return instance;
    }

    internal void DestroyAllSystems() {
        var systems = _systemLookup.Values.ToArray();

        foreach (var system in systems) {
            DestroySystem(system);
        }
    }

    private void CreateSystemInternal(SystemBase system) {
        try {
            system.AssemblyNameString = CurrentAssemblyName.Name;
            system.CreateInstance(this);
        }
        catch {
            try {
                system.DestroyInstance();
            }
            catch (Exception ex) {
                Logger.Error(ex, $"Error destroying system during rollback {system.GetType().Name}");
            }

            try {
                SystemDestroyed?.Invoke(this, system);
            }
            catch (Exception ex) {
                Logger.Error(ex, $"SystemDestroyed event handler threw during rollback");
            }

            RemoveSystemInternal(system);
            throw;
        }

        try {
            SystemCreated?.Invoke(this, system);
        }
        catch (Exception ex) {
            Logger.Error(ex, "SystemCreated event handler threw an exception");
        }
    }

    private void RemoveSystemInternal(SystemBase system) {
        if (system == null)
            return;

        var type = system.GetType();
        _systemLookup.Remove(type);
    }
}