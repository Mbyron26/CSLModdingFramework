using System;
using ColossalFramework.UI;
using CSLModdingFramework.Configuration;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.UI;
using CSLModdingFramework.Utilities;
using ICities;
using UnityEngine;

namespace CSLModdingFramework.Core;

public abstract class OptionsPanelSystemBase : SystemBase {
    private Type _panelType;
    private UIComponent _panel;
    private UIPanel _container;

    private SettingsSystem _settingsSystem;

    protected abstract Type GetOptionsPanelType();

    protected virtual void OnPanelDestroy() => _settingsSystem.SaveMainSettings();

    protected override void OnCreate() {
        base.OnCreate();

        _settingsSystem = Domain.ModSystem.ModSettingsSystem;
        _panelType = GetOptionsPanelType();
    }

    public void OnLocaleChanged() {
        if (_container == null || !_container.isVisible) return;

        Destroy();
        Create();
    }

    public void SettingsUI(UIHelperBase helper) {
        if (((UIHelper)helper).self is not UIScrollablePanel scrollablePanel) {
            Logger.Error("OptionsPanelSystem.SettingsUI: UIScrollablePanel is null");
            return;
        }

        scrollablePanel.autoLayout = false;
        _container = scrollablePanel.parent as UIPanel;
        if (_container == null) {
            Logger.Error("OptionsPanelSystem.SettingsUI: Container is null");
            return;
        }

        LinqExtensions.ForEach(_container.components, component => component.isVisible = false);
        _container.autoLayout = false;
        _container.eventVisibilityChanged += OnContainerVisibilityChanged;
    }

    private void Create() {
        try {
            if (_panelType is null || _panel != null) return;

            using var pc = PerformanceCounter.Start(c => Logger.Verbose($"OptionsPanelSystem create panel: {c.Milliseconds}"));
            _panel = _container.AddUIComponent(_panelType);
            _panel.relativePosition = Vector2.zero;
        }
        catch (Exception e) {
            Logger.Error(e, "Create options panel object failed");
        }
    }

    private void Destroy() {
        if (_panelType == null || _panel == null) return;

        using var pc = PerformanceCounter.Start(c => Logger.Verbose($"OptionsPanelManager destroy panel: {c.Milliseconds}"));

        OnPanelDestroy();
        _panel.DestroySelf();
        _panel = null;
    }

    private void OnContainerVisibilityChanged(UIComponent _, bool isVisible) {
        if (isVisible)
            Create();
        else
            Destroy();
    }
}