using ColossalFramework.Plugins;
using ColossalFramework.UI;
using CSLModdingFramework.Configuration;
using CSLModdingFramework.Core;
using CSLModdingFramework.UI;
using CSLModdingFramework.UI.Controls;
using CSLModdingFramework.UI.Utilities;
using UnifiedUI.Helpers;
using UnityEngine;

namespace CSLModdingFramework.ToolPanel;

public abstract class UnifiedUIToolPanelSystemBase : UISystemBase {
    protected SettingsSystem _settingManager;
    protected ModSettingsBase _defaultSetting;

    private Texture2D _unifiedUIIcon;
    protected abstract IAtlas ButtonAtlas { get; }
    protected abstract string ButtonSpriteName { get; }
    protected abstract KeyBinding TriggerKeyBinding { get; }

    protected virtual string Tooltip => TriggerKeyBinding is null ? string.Empty : $"{Domain.ModSystem.ModName} ({TriggerKeyBinding})";

    public bool IsUnifiedUIModEnabled { get; protected set; }
    public Toggle InGameToolButton { get; protected set; }
    protected Texture2D UnifiedUIIcon => _unifiedUIIcon ??= GetIconTexture();
    protected UUICustomButton UnifiedUIButton { get; set; }

    protected override void OnCreate() {
        base.OnCreate();

        _settingManager = Domain.GetOrCreateSystem<SettingsSystem>();
        _defaultSetting = _settingManager.MainSettings;
        PluginManager.instance.eventPluginsChanged += CheckUnifiedUIModIsEnabled;
        PluginManager.instance.eventPluginsStateChanged += CheckUnifiedUIModIsEnabled;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        
        PluginManager.instance.eventPluginsChanged -= CheckUnifiedUIModIsEnabled;
        PluginManager.instance.eventPluginsStateChanged -= CheckUnifiedUIModIsEnabled;
    }

    protected override void OnGameLoaded(LoadContext loadContext) {
        base.OnGameLoaded(loadContext);

        // CheckUnifiedUIModIsEnabled();
        if (loadContext.Target is not (LoadTarget.Game or LoadTarget.Map or LoadTarget.Asset)) return;

        Enable();
    }

    protected override void OnGameUnloaded() {
        base.OnGameUnloaded();
        
        Disable();
    }

    protected virtual Texture2D GetIconTexture() => TextureLoader.FromAssemblySafe($"{AssemblyNameString}.UI.Resources.InGameButton.png", GetType().Assembly);

    protected virtual void Enable() {
        // CheckUnifiedUIModIsEnabled();

        if (_settingManager.MainSettings is not ToolButtonModSettingsBase settings) {
            Logger.Error($"Enable: {_defaultSetting.GetType().Name} does not inherit from ToolButtonModSettingsBase");
            return;
        }

        if (!IsUnifiedUIModEnabled && settings.ToolButtonDisplayScope is ToolButtonScope.UnifiedUI or ToolButtonScope.Both) {
            settings.ToolButtonDisplayScope = ToolButtonScope.InGame;
            Logger.Debug("Enable: Changed tool button display scope to InGame");
            _settingManager.SaveMainSettings();
        }

        if (IsUnifiedUIModEnabled) {
            switch (settings.ToolButtonDisplayScope) {
                case ToolButtonScope.InGame:
                    AddInGameToolButton();
                    break;
                case ToolButtonScope.UnifiedUI:
                    AddUnifiedUIButton();
                    break;
                case ToolButtonScope.Both:
                    AddInGameToolButton();
                    AddUnifiedUIButton();
                    break;
            }
        }
        else {
            if (settings.ToolButtonDisplayScope == ToolButtonScope.InGame)
                AddInGameToolButton();
        }
    }

    protected virtual void Disable() {
        // CheckUnifiedUIModIsEnabled();
        RemoveUnifiedUIButton();
        RemoveInGameToolButton();
    }

    public void OnToolButtonSelectionChanged() {
        if (CurrentMode == GameMode.MainMenu) return;

        Disable();
        Enable();
    }

    public void ClosePanel() {
        if (!HasPanel) return;

        if (UnifiedUIButton != null && UnifiedUIButton.Button != null) {
            UnifiedUIButton.IsPressed = false;
        }

        if (InGameToolButton != null) {
            InGameToolButton.SetToggleStateWithoutNotify(false);
        }

        DestroyPanelObject();
    }

    public void KeyBindingTriggered() {
        if (HasPanel) {
            if (UnifiedUIButton != null && UnifiedUIButton.Button != null) {
                UnifiedUIButton.IsPressed = false;
            }

            if (InGameToolButton != null) {
                InGameToolButton.SetToggleStateWithoutNotify(false);
            }

            DestroyPanelObject();
        }
        else {
            if (UnifiedUIButton != null && UnifiedUIButton.Button != null) {
                UnifiedUIButton.IsPressed = true;
            }

            if (InGameToolButton != null) {
                InGameToolButton.SetToggleStateWithoutNotify(true);
            }

            CreatePanelObject();
        }
    }

    #region InGame Tool Button

    public virtual Vector3 GetDefaultInGameToolButtonPosition() => Vector3.zero;

    public virtual void SetInGameToolButtonDefaultPosition() {
        if (InGameToolButton == null) return;

        InGameToolButton.relativePosition = GetDefaultInGameToolButtonPosition();
        SaveInGameToolButtonPosition(InGameToolButton.relativePosition);
    }

    protected virtual void AddInGameToolButton() {
        if (InGameToolButton != null) return;

        InGameToolButton = UIView.GetAView().AddUIComponent<Toggle>();
        InGameToolButton.tooltipBox = UIView.GetAView()?.defaultTooltipBox;
        InGameToolButton.tooltip = Tooltip;
        InGameToolButton.ToggleChanged += OnInGameButtonToggle;
        SetupInGameToolButton(InGameToolButton);
        Logger.Debug("Added In game tool button");
    }

    protected virtual void RemoveInGameToolButton() {
        if (InGameToolButton == null) return;

        InGameToolButton.ToggleChanged -= OnInGameButtonToggle;
        InGameToolButton.eventMouseMove -= OnInGameToolButtonMouseMove;
        InGameToolButton.eventMouseUp -= OnInGameToolButtonMouseUp;
        InGameToolButton.DestroySelf();
        InGameToolButton = null;
        Logger.Debug("Removed In game tool button");
    }

    protected virtual void SetupInGameToolButton(Toggle toggle) {
        toggle.size = new Vector2(40, 40);
        toggle.eventMouseMove += OnInGameToolButtonMouseMove;
        toggle.eventMouseUp += OnInGameToolButtonMouseUp;
        if (_defaultSetting is ToolButtonModSettingsBase setting) {
            var settingPositionX = setting.ToolButtonPositionX;
            var settingPositionY = setting.ToolButtonPositionY;

            var resolution = UIView.GetAView().GetScreenResolution();
            var defaultPosition = GetDefaultInGameToolButtonPosition();

            var newPosition = new Vector2(settingPositionX, settingPositionY);

            if (settingPositionX > resolution.x) {
                newPosition.x = defaultPosition.x;
            }

            if (settingPositionY > resolution.y) {
                newPosition.y = defaultPosition.y;
            }

            toggle.relativePosition = newPosition;
        }


        var bg = toggle.Background;
        bg.Atlas = DefaultAtlas.Instance;

        bg.OffSprites.SetValues(DefaultAtlas.Circle);
        bg.OffColors.SetValues(new Color32(0, 0, 0, 0), new Color32(50, 50, 50, 100), new Color32(70, 70, 70, 100), new Color32(70, 70, 70, 100), new Color32(0, 0, 0, 0));

        bg.OnSprites.SetValues(DefaultAtlas.Circle);
        bg.OnColors.SetValues(UIColorPalette.GreenFocused);
        bg.OnColors.Hovered = UIColorPalette.GreenHovered;

        var fg = toggle.Foreground;
        fg.Atlas = ButtonAtlas;
        fg.OffSprites.SetValues(ButtonSpriteName);
        fg.OnSprites.SetValues(ButtonSpriteName);
    }

    protected void SaveInGameToolButtonPosition(Vector3 position) {
        if (_defaultSetting is ToolButtonModSettingsBase settings) {
            settings.ToolButtonPositionX = position.x;
            settings.ToolButtonPositionY = position.y;
            _settingManager.SaveMainSettings();
        }
        else {
            Logger.Error($"SaveInGameToolButtonPosition: {_defaultSetting.GetType().Name} does not inherit from ToolButtonModSettingsBase");
        }
    }

    protected virtual void OnInGameButtonToggle(Toggle toggle, bool isOn) {
        if (UnifiedUIButton != null && UnifiedUIButton.Button != null) {
            UnifiedUIButton.IsPressed = isOn;
        }

        TogglePanelVisibility();
    }

    private void OnInGameToolButtonMouseMove(UIComponent component, UIMouseEventParameter eventParam) {
        if (!eventParam.buttons.IsFlagSet(UIMouseButton.Right)) return;

        var ratio = UIView.GetAView().ratio;
        var togglePosition = component.position;
        component.position = new Vector3(togglePosition.x + eventParam.moveDelta.x * ratio, togglePosition.y + eventParam.moveDelta.y * ratio, togglePosition.z);
    }

    private void OnInGameToolButtonMouseUp(UIComponent component, UIMouseEventParameter eventParam) {
        if (!eventParam.buttons.IsFlagSet(UIMouseButton.Right)) return;

        SaveInGameToolButtonPosition(new Vector3(component.relativePosition.x, component.relativePosition.y));
    }

    #endregion

    #region UnifiedUI Button

    private void AddUnifiedUIButton() {
        if (!IsUnifiedUIModEnabled || UnifiedUIButton != null) return;

        UnifiedUIButton = UUIHelpers.RegisterCustomButton(AssemblyNameString, null, Tooltip, UnifiedUIIcon, OnUUIButtonToggle);
        UnifiedUIButton.Button.eventTooltipEnter += (c, _) => c.tooltip = Tooltip;
        UnifiedUIButton.IsPressed = false;

        Logger.Debug("Added UnifiedUI button");
    }

    private void RemoveUnifiedUIButton() {
        if (!IsUnifiedUIModEnabled || UnifiedUIButton == null) return;

        UnifiedUIButton.Button.Destroy();
        UnifiedUIButton = null;
        Logger.Debug("Removed UnifiedUI button");
    }

    protected virtual void OnUUIButtonToggle(bool isOn) {
        if (InGameToolButton != null) {
            InGameToolButton.SetToggleStateWithoutNotify(isOn);
        }

        TogglePanelVisibility();
    }

    #endregion

    private void CheckUnifiedUIModIsEnabled() => IsUnifiedUIModEnabled = Utilities.PluginHelper.IsPluginEnabled("UnifiedUIMod");
}