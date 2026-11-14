namespace CSLModdingFramework.Configuration;

public interface ISettingsSystem {
    T LoadOrCreateMainSettings<T>() where T : ModSettingsBase, new();

    T LoadOrCreateSettings<T>() where T : ISettings, new();

    void SaveAllSettings();
    void SaveMainSettings();
    void SaveSettings<T>() where T : ISettings;
    bool SetMainSettingsDefaults();
    bool SetSettingsDefaults<T>() where T : ISettings, new();
}