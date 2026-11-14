using System;
using System.Linq;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Localization;
using CSLModdingFramework.UI.Dialogs;
using CSLModdingFramework.Utilities;

namespace CSLModdingFramework.Core.Compatibility;

public class CompatibilitySystem : SystemBase {
    private ModSystemBase _modSystem;
    private string _modName;
    private CompatibilityStatus _currentStatus;
    private bool _processingIncompatibilityMods;

    public bool ShouldRestartGame { get; private set; }
    internal DependencyModRule DependencyRule { get; private set; }
    internal VersionModRule VersionRule { get; private set; }
    internal IncompatibleModRule IncompatibleRule { get; private set; }
    public CompatibilityStatus CurrentStatus => _currentStatus;

    protected override void OnCreate() {
        base.OnCreate();

        _modSystem = Domain.ModSystem;
        _modName = _modSystem.ModName;
        DependencyRule = new DependencyModRule();
        VersionRule = new VersionModRule();
        IncompatibleRule = new IncompatibleModRule(Logger);
        _modSystem.ResisterCompatibilityInfo(DependencyRule, VersionRule, IncompatibleRule);
        
        if (UIView.GetAView() != null) {
            CheckAndShowDialogIfNeeded();
        }
        else {
            LoadingManager.instance.m_introLoaded += CheckAndShowDialogIfNeeded;
        }
        
        PluginManager.instance.eventPluginsStateChanged += CheckAndShowDialogIfNeeded;
    }

    public void CheckAndShowDialog() {
        CheckCompatibility();
        ShowCompatibilityDialog();
    }

    internal void CheckAndShowDialogIfNeeded() {
        if (_processingIncompatibilityMods) return;

        CheckCompatibility();
        if (_currentStatus.HasStatus(CompatibilityStatus.IncompatibleMods))
            ShowCompatibilityDialog();
    }

    internal void OnHandleIncompatibleMods() {
        if (IncompatibleRule.Count == 0) return;

        _processingIncompatibilityMods = true;

        IncompatibleRule.Handle();
        ShouldRestartGame = true;
        ShowRestartGameDialog();

        _processingIncompatibilityMods = false;
    }

    private void ShowCompatibilityDialog() => CompatibilityDialog.ShowCompatibilityDialog(Domain);

    private void ShowRestartGameDialog() => MessageDialog.ShowMessageDialog(_modName, _modSystem.ModLocalizationSystem.GetString(LocalizationStrings.RestartGame), _modSystem.ModLocalizationSystem.GetString(LocalizationStrings.OK));

    private void CheckCompatibility() {
        if (!PluginManager.exists) return;
        using var pc = PerformanceCounter.Start(v => Logger.Verbose($"Check compatibility took {v.Milliseconds}"));
        try {
            _currentStatus = CompatibilityStatus.Normal;
            var pluginsInfo = PluginManager.instance.GetPluginsInfo();
            if (pluginsInfo is null) {
                Logger.Error("PluginManager.GetPluginsInfo() returned null");
                return;
            }
            var pluginInfos = pluginsInfo as PluginManager.PluginInfo[] ?? pluginsInfo.ToArray();
           
            DependencyRule.Check(pluginInfos, ref _currentStatus);
            VersionRule.Check(pluginInfos, ref _currentStatus);
            IncompatibleRule.Check(pluginInfos, ref _currentStatus);
            Logger.Info($"Current mod Status: {_currentStatus}");
        }catch (Exception ex) {
            Logger.Error(ex);
            _currentStatus = CompatibilityStatus.UnknownError;
        }
        
    }
}