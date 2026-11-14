using System;
using System.Reflection;
using CSLModdingFramework.Configuration;
using CSLModdingFramework.Core.Compatibility;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Localization;
using CSLModdingFramework.Logging;
using CSLModdingFramework.Serialization;
using CSLModdingFramework.UI.Dialogs;
using ICities;

namespace CSLModdingFramework.Core;

public abstract class ModSystemBase : SystemBase {
    protected CompatibilitySystem _compatibilitySystem;
    protected LocalizationSystem _modLocalizationSystem;
    protected SettingsSystem _modSettingsSystem;
    protected UpdateSystem _modUpdateSystem;
    private ChangelogCollection _changelogs;
    private AssemblyName _currentAssemblyName;
    private Version _modVersion;
    private string _rowName;
    public abstract DateTime BuildDate { get; }
    public abstract ReleaseChannel Channel { get; }

    public abstract string ModName { get; }
    public virtual string DiscordURL => "https://discord.gg/ZxbyzmvGxG";
    public virtual string ModDescription => string.Empty;
    public virtual string ModSteamURL => string.Empty;
    public virtual string ModTranslationURL => string.Empty;
    public virtual Version ModVersion => _modVersion ??= CurrentAssemblyName.Version;
    public virtual string SharedTranslationURL => "https://crowdin.com/project/csshared";
    public ChangelogCollection Changelog => _changelogs ??= AddChangelogsInternal();
    public CompatibilitySystem CompatibilitySystem => _compatibilitySystem;
    public AssemblyName CurrentAssemblyName => _currentAssemblyName ??= GetType().Assembly.GetName();
    public bool IsEnabled { get; private set; }
    public string ModDirectory { get; private set; }
    public LocalizationSystem ModLocalizationSystem => _modLocalizationSystem;
    public SettingsSystem ModSettingsSystem => _modSettingsSystem;
    public UpdateSystem ModUpdateSystem => _modUpdateSystem;
    public string RowName => _rowName ??= CurrentAssemblyName.Name;

    protected abstract void CreateSettings(ISettingsSystem settingsSystem);
    protected abstract void OnSettingsUI(UIHelperBase helper);

    protected virtual void AddChangelogs(ChangelogCollection changelogCollection) { }
    protected virtual void AddDependencyModRule(IDependencyModRule rule) { }
    protected virtual void AddIncompatibleModRule(IIncompatibleModRule rule) { }
    protected virtual void AddVersionModRule(IVersionModRule rule) { }
    protected virtual void OnDisabled() { }
    protected virtual void OnEnabled() { }
    protected virtual void OnUpdateSystem(UpdateSystem updateSystem) { }
    protected virtual void SetupLogger(ILog logger) { }

    protected override void OnCreate() {
        base.OnCreate();

        SetupLogger(Logger);
        ModDirectory = Utilities.PluginHelper.GetPluginDirectory(CurrentAssemblyName.Name);
        if (!string.IsNullOrEmpty(ModDirectory))
            Logger.Info($"Mod directory: {ModDirectory}");
    }

    protected override void OnGameLoaded(LoadContext context) {
        base.OnGameLoaded(context);

        if (IsEnabled)
            ShowLogDialog();
    }

    internal void Disable() {
        Logger.Info("Disabling mod");

        IsEnabled = false;
        OnDisabled();
        foreach (var systemBase in Domain._systemLookup) {
            systemBase.Value.IsActive = false;
        }

        Logger.Info("Mod disabled");
    }

    internal void Enable() {
        Logger.Info("Enabling mod");

        IsEnabled = true;
        _modUpdateSystem = Domain.GetOrCreateSystem<UpdateSystem>();
        _modSettingsSystem = Domain.GetOrCreateSystem<SettingsSystem>();
        CreateSettings(_modSettingsSystem);

        _modLocalizationSystem = Domain.GetOrCreateSystem<LocalizationSystem>();
        _compatibilitySystem = Domain.GetOrCreateSystem<CompatibilitySystem>();
        Domain.GetOrCreateSystem<SerializationSystem>();

        OnUpdateSystem(_modUpdateSystem);

        OnEnabled();

        foreach (var systemBase in Domain._systemLookup) {
            systemBase.Value.IsActive = true;
        }

        Logger.Info("Mod enabled");
    }

    internal string GetModEntryDescription() => _modLocalizationSystem != null ? _modLocalizationSystem.GetString(LocalizationSystem.ModDescriptionLocalizedId) : ModDescription;

    internal string GetModName() => Channel switch {
        ReleaseChannel.Alpha => $"{ModName} [ALPHA] {ModVersion.ToDisplayString()}",
        ReleaseChannel.Beta => $"{ModName} [BETA] {ModVersion.ToDisplayString()}",
        _ => $"{ModName} {ModVersion.ToDisplayString()}"
    };

    internal void ResisterCompatibilityInfo(IDependencyModRule dependencyModRule, IVersionModRule versionModRule, IIncompatibleModRule incompatibleModRule) {
        AddDependencyModRule(dependencyModRule);
        AddVersionModRule(versionModRule);
        AddIncompatibleModRule(incompatibleModRule);
    }

    internal void SettingsUI(UIHelperBase helper) => OnSettingsUI(helper);

    private ChangelogCollection AddChangelogsInternal() {
        _changelogs = new ChangelogCollection(_modLocalizationSystem);
        AddChangelogs(_changelogs);
        return _changelogs;
    }

    private void ShowLogDialog() {
        if (!IsEnabled) return;

        var defaultSetting = _modSettingsSystem.MainSettings;

        var lastVersion = defaultSetting.CurrentModVersion;
        var modVersion = Core.ModVersion.FromVersion(ModVersion);

        if (Channel != ReleaseChannel.Alpha && Channel != ReleaseChannel.Beta) {
            if (modVersion.CompareWithoutRevision(lastVersion) > 0) {
                ChangelogDialog.ShowChangelogDialog(Domain);
            }
        }

        defaultSetting.CurrentModVersion = modVersion;
        _modSettingsSystem.SaveMainSettings();
    }
}