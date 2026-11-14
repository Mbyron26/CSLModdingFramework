using CitiesHarmony.API;
using CSLModdingFramework.Core;
using CSLModdingFramework.Core.Compatibility;
using CSLModdingFramework.Localization;
using CSLModdingFramework.UI.Dialogs;

namespace CSLModdingFramework.HarmonyPatch;

public abstract class HarmonyModSystemBase : ModSystemBase {
    public virtual string HarmonyID => AssemblyNameString;
    public bool IsPatched { get; private set; }
    public HarmonyPatcher Patcher { get; private set; }

    protected override void OnEnabled() => ApplyPatches();

    protected override void OnDisabled() => RemovePatches();

    protected override void AddDependencyModRule(IDependencyModRule rule) => rule.Add("CitiesHarmony.Harmony", "Harmony");

    private void ApplyPatches() {
        if (IsPatched) return;

        if (HarmonyHelper.IsHarmonyInstalled) {
            Logger.Info("Starting Harmony patches");
            Patcher = new HarmonyPatcher(HarmonyID, Logger);
            Patcher.ApplyAll(RegisterPatches);
            IsPatched = true;
            Logger.Info("Harmony patches completed");
        }
        else {
            MessageDialog.ShowMessageDialog(Domain, ModLocalizationSystem.GetString(LocalizationStrings.MissingHarmonyDependency));
            Logger.Fatal("Harmony is not installed correctly");
        }
    }

    private void RemovePatches() {
        if (!IsPatched || !HarmonyHelper.IsHarmonyInstalled) return;

        Logger.Info("Reverting Harmony patches");
        Patcher.RemoveAll();
        Patcher = null;
        IsPatched = false;
    }

    protected virtual void RegisterPatches(HarmonyPatcher harmonyPatcher) { }
}