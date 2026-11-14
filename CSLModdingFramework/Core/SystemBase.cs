using ColossalFramework;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Logging;
using ICities;

namespace CSLModdingFramework.Core;

public abstract class SystemBase {
    protected internal virtual bool UpdateEnabled { get; set; }
    public string AssemblyNameString { get; internal set; }
    public GameMode CurrentMode => GetCurrentGameMode();
    public ModDomain Domain { get; private set; }
    public bool IsActive { get; internal set; }
    public bool IsGameLoaded { get; private set; }
    public ILog Logger { get; protected set; }

    protected internal virtual void OnStartRunning() { }
    protected internal virtual void OnStopRunning() { }
    protected internal virtual void OnUpdate() { }
    protected internal virtual float UpdateInterval() => 0f;

    protected virtual void OnCreate() {
        Logger = LogManager.GetLogger(AssemblyNameString);

        LoadingHook.GamePreLoad += GamePreLoad;
        LoadingHook.GameLoaded += GameLoaded;
        LoadingHook.GamePreUnload += GamePreUnload;
        LoadingHook.GameUnloaded += GameUnloaded;

        Logger.Info($"{GetType().Name} created");
    }

    protected virtual void OnDestroy() {
        LoadingHook.GamePreLoad -= GamePreLoad;
        LoadingHook.GameLoaded -= GameLoaded;
        LoadingHook.GamePreUnload -= GamePreUnload;
        LoadingHook.GameUnloaded -= GameUnloaded;
    }

    protected virtual void OnGameLoaded(LoadContext context) { }
    protected virtual void OnGamePreLoad() { }
    protected virtual void OnGamePreUnload() { }
    protected virtual void OnGameUnloaded() { }

    internal virtual void CreateInstance(ModDomain domain) {
        Domain = domain;
        OnCreate();
    }

    internal virtual void DestroyInstance() {
        OnDestroy();
        Domain = null;
    }

    private void GameLoaded(LoadMode mode) {
        if (!IsActive) return;

        IsGameLoaded = true;
        OnGameLoaded(mode.ToLoadContext());
    }

    private void GamePreLoad() {
        if (!IsActive) return;

        OnGamePreLoad();
    }

    private void GamePreUnload() {
        if (!IsActive) return;

        OnGamePreUnload();
    }

    private void GameUnloaded() {
        if (!IsActive) return;

        IsGameLoaded = false;
        OnGameUnloaded();
    }

    private GameMode GetCurrentGameMode() {
        if (!Singleton<ToolManager>.exists) return GameMode.MainMenu;

        var toolManagerProperties = Singleton<ToolManager>.instance.m_properties;
        if (toolManagerProperties == null) return GameMode.MainMenu;

        return toolManagerProperties.m_mode switch {
            ItemClass.Availability.Game => GameMode.Game,
            ItemClass.Availability.MapEditor => GameMode.MapEditor,
            ItemClass.Availability.AssetEditor => GameMode.AssetEditor,
            ItemClass.Availability.ThemeEditor => GameMode.ThemeEditor,
            ItemClass.Availability.ScenarioEditor => GameMode.ScenarioEditor,
            _ => GameMode.MainMenu
        };
    }
}