using ICities;

namespace CSLModdingFramework.Core;

public abstract class ModEntry<TManager> : IUserMod where TManager : ModSystemBase, new() {
    private TManager _modManager;

    protected TManager ModManager => _modManager ??= CreateManager();

    public string Name => ModManager.GetModName();
    public string Description => ModManager.GetModEntryDescription();
    
    public void OnEnabled() => ModManager.Enable();

    public void OnDisabled() => ModManager.Disable();

    public void OnSettingsUI(UIHelperBase helper) => ModManager.SettingsUI(helper);

    private TManager CreateManager() => (TManager)ModDomain.GetOrCreateDomain<TManager>().ModSystem;
}