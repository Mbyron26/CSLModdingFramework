using System;
using System.Collections.Generic;
using System.IO;
using ColossalFramework.IO;
using CSLModdingFramework.Core;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Utilities;

namespace CSLModdingFramework.Configuration;

public sealed class SettingsSystem : SystemBase, ISettingsSystem {
    private Dictionary<Type, SettingsSource> _extraSettings;
    
    public static string ModsSettingsDirectory { get; } = Path.Combine(DataLocation.localApplicationData, "ModsSettings");
    public ModSettingsBase MainSettings { get; private set; }
    public SettingsSource MainSettingsSource { get; private set; }

    public string ModSettingsDirectory { get; private set; }

    protected override void OnCreate() {
        base.OnCreate();

        _extraSettings = new Dictionary<Type, SettingsSource>();
        ModSettingsDirectory = PathExtensions.Combine(ModsSettingsDirectory, AssemblyNameString);
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        _extraSettings.Clear();
        _extraSettings = null;
        MainSettingsSource = null;
        MainSettings = null;
    }

    public static string GetFileName<T>() => ((SettingsFileAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(SettingsFileAttribute)))?.FileName;

    public T GetMainSettings<T>() where T : ModSettingsBase => MainSettings as T;

    public T GetSettings<T>() where T : ISettings {
        if (_extraSettings.TryGetValue(typeof(T), out var src))
            return (T)src.Settings;

        return default;
    }

    public T LoadOrCreateMainSettings<T>() where T : ModSettingsBase, new() {
        var type = typeof(T);
        if (!type.IsSubclassOf(typeof(ModSettingsBase)) || type.IsAbstract) return null;

        var fileName = GetFileName<T>();

        if (string.IsNullOrEmpty(fileName)) {
            Logger.Error("Loading main settings failed because no file location was provided.");
            return null;
        }

        T settings;

        var path = PathExtensions.Combine(ModsSettingsDirectory, AssemblyNameString, fileName);

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (File.Exists(path)) {
            try {
                var json = File.ReadAllText(path);
                var instance = JsonHelper.Deserialize<T>(json);
                if (instance is not null) {
                    settings = instance;
                    Logger.Info($"Local main setting detected, deserialized, path: {path}");
                }
                else {
                    settings = new T();
                    settings.SetDefaults();
                    Logger.Warn($"Main settings file deserialized to null, using default instance. Path: {path}");
                }
            }
            catch (Exception e) {
                Logger.Error(e, $"Failed to deserialize main settings file, using default instance. Path: {path}");
                settings = new T();
            }
        }
        else {
            settings = new T();
            Logger.Info($"No main settings file found, using default instance. Path: {path}");
        }

        MainSettings = settings;
        MainSettingsSource = new SettingsSource(path, settings);
        _extraSettings[type] = MainSettingsSource;

        Logger.Info($"Loaded [{type.FullName}] setting");
        return settings;
    }

    public T LoadOrCreateSettings<T>() where T : ISettings, new() {
        var type = typeof(T);

        if (type.IsSubclassOf(typeof(ModSettingsBase)) || type.IsAbstract)
            return default;

        var fileName = GetFileName<T>();
        if (string.IsNullOrEmpty(fileName)) {
            Logger.Error($"Loading extra settings failed because no file location was provided. Type: {type.FullName}");
            return default;
        }

        var path = PathExtensions.Combine(ModsSettingsDirectory, AssemblyNameString, fileName);

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        T settings;

        if (File.Exists(path)) {
            try {
                var json = File.ReadAllText(path);
                var instance = JsonHelper.Deserialize<T>(json);
                if (instance is not null) {
                    settings = instance;
                    Logger.Info($"Extra settings detected, deserialized. Path: {path}");
                }
                else {
                    settings = new T();
                    settings.SetDefaults();
                    Logger.Warn($"Extra settings deserialized to null, using default instance. Path: {path}");
                }
            }
            catch (Exception e) {
                Logger.Error(e, $"Failed to deserialize extra settings file, using default instance. Path: {path}");
                settings = new T();
                settings.SetDefaults();
            }
        }
        else {
            settings = new T();
            settings.SetDefaults();
            Logger.Info($"No extra settings file found, using default instance. Path: {path}");
        }

        var source = new SettingsSource(path, settings);
        _extraSettings[type] = source;

        Logger.Info($"Loaded extra settings [{type.FullName}]");
        return settings;
    }

    public void SaveAllSettings() {
        if (MainSettingsSource is not null) {
            try {
                var json = JsonHelper.Serialize(MainSettingsSource.Settings);
                File.WriteAllText(MainSettingsSource.Path, json);
                Logger.Verbose($"Main settings saved. Path: {MainSettingsSource.Path}");
            }
            catch (Exception e) {
                Logger.Error(e, $"Failed to save main settings. Path: {MainSettingsSource.Path}");
            }
        }

        foreach (var kv in _extraSettings) {
            var type = kv.Key;
            var source = kv.Value;

            if (ReferenceEquals(source, MainSettingsSource))
                continue;

            try {
                var json = JsonHelper.Serialize(source.Settings);
                File.WriteAllText(source.Path, json);
                Logger.Info($"Settings saved. Type: {type.FullName}, Path: {source.Path}");
            }
            catch (Exception e) {
                Logger.Error(e, $"Failed to save settings. Type: {type.FullName}, Path: {source.Path}");
            }
        }
    }

    public void SaveMainSettings() {
        if (MainSettingsSource is null) {
            Logger.Warn("SaveMainSettings called but MainSettingsSource is null.");
            return;
        }

        try {
            var json = JsonHelper.Serialize(MainSettingsSource.Settings);
            File.WriteAllText(MainSettingsSource.Path, json);
            Logger.Debug($"Main settings saved. Path: {MainSettingsSource.Path}");
        }
        catch (Exception e) {
            Logger.Error(e, $"Failed to save main settings. Path: {MainSettingsSource.Path}");
        }
    }

    public void SaveSettings<T>() where T : ISettings {
        var type = typeof(T);

        if (!_extraSettings.TryGetValue(type, out var source)) {
            Logger.Warn($"SaveSettings called but no settings found for type: {type.FullName}");
            return;
        }

        try {
            var json = JsonHelper.Serialize(source.Settings);
            File.WriteAllText(source.Path, json);
            Logger.Debug($"Settings saved. Type: {type.FullName}, Path: {source.Path}");
        }
        catch (Exception e) {
            Logger.Error(e, $"Failed to save extra settings. Type: {type.FullName}, Path: {source.Path}");
        }
    }

    public bool SetMainSettingsDefaults() {
        if (MainSettings is null) {
            Logger.Warn("SetMainSettingsDefaults called but MainSettings is null.");
            return false;
        }

        try {
            MainSettings.SetDefaults();
            Logger.Info($"Defaults applied to main settings [{MainSettings.GetType().FullName}].");
            return true;
        }
        catch (Exception e) {
            Logger.Error(e, $"Failed to apply defaults to main settings [{MainSettings.GetType().FullName}].");
            return false;
        }
    }

    public bool SetSettingsDefaults<T>() where T : ISettings, new() {
        var type = typeof(T);

        if (type.IsSubclassOf(typeof(ModSettingsBase))) {
            Logger.Warn($"SetSettingsDefaults should not be used on main settings type: {type.FullName}");
            return false;
        }

        if (!_extraSettings.TryGetValue(type, out var source)) {
            Logger.Warn($"SetSettingsDefaults called but no settings found for type: {type.FullName}");
            return false;
        }

        try {
            if (source.Settings is T settings) {
                settings.SetDefaults();
                Logger.Info($"Defaults applied to settings [{type.FullName}].");
                return true;
            }

            Logger.Warn($"SettingsSource.Settings is not of expected type {type.FullName}.");
            return false;
        }
        catch (Exception e) {
            Logger.Error(e, $"Failed to apply defaults to extra settings [{type.FullName}].");
            return false;
        }
    }
}