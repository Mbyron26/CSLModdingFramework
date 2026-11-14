using CSLModdingFramework.Core;
using CSLModdingFramework.Localization;

namespace CSLModdingFramework.Configuration;

public abstract class ModSettingsBase : IModSettings {
    public ModVersion CurrentModVersion { get; set; }
    public string LocaleId { get; set; } = LocalizationSystem.UseGameLanguage;

    public virtual void SetDefaults() {
        CurrentModVersion = new ModVersion();
        LocaleId = LocalizationSystem.UseGameLanguage;
    }
}