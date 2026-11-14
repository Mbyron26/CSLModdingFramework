using System;
using ColossalFramework.UI;
using CSLModdingFramework.UI;
using CSLModdingFramework.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CSLModdingFramework.Core;

public abstract class UISystemBase : SystemBase {
    protected GameObject _panelGameObject;

    public event UIEventHandler<UIComponent> PanelCreated;
    public event Action PanelDestroying;
    public event Action PanelDestroyed;

    public abstract Type PanelType { get; }
    public virtual bool ShowWhenCreated => true;
    public bool HasPanel => Panel != null;

    public UIComponent Panel { get; protected set; }

    public virtual void BringToFront() => Panel?.BringToFront();

    public virtual void SendToBack() => Panel?.SendToBack();

    protected virtual void OnPanelCreated(UIComponent panel) => PanelCreated?.Invoke(panel);

    protected virtual void OnPanelDestroyed() => PanelDestroyed?.Invoke();

    protected virtual void OnPanelDestroying() => PanelDestroying?.Invoke();

    public virtual void EnsurePanelVisible() {
        if (Panel == null) {
            CreatePanelObject();
            return;
        }

        ShowPanel();
    }

    public virtual void RecreatePanel() {
        DestroyPanelObject();
        CreatePanelObject();
    }

    public virtual void RecreatePanelIfVisible() {
        if (Panel == null || !Panel.isVisible) return;

        RecreatePanel();
    }

    public virtual void TogglePanelVisibility() {
        if (Panel != null && Panel.isVisible)
            HidePanel();
        else
            EnsurePanelVisible();
    }

    public virtual void TogglePanelObject() {
        if (Panel == null)
            CreatePanelObject();
        else
            DestroyPanelObject();
    }

    public virtual UIComponent CreatePanelObject() {
        if (Panel != null) return Panel;

        using var performanceCounter = PerformanceCounter.Start(c => Logger.Verbose($"{PanelType.FullName} create time: {c.Milliseconds}"));

        _panelGameObject = new GameObject(PanelType.FullName) {
            transform = {
                parent = UIView.GetAView().transform
            }
        };
        
        Panel = (UIComponent)_panelGameObject.AddComponent(PanelType);
        
        if (ShowWhenCreated)
            ShowPanel();
        else
            HidePanel();

        OnPanelCreated(Panel);
        return Panel;
    }

    public virtual void DestroyPanelObject() {
        if (Panel == null) return;

        using var performanceCounter = PerformanceCounter.Start(c => Logger.Verbose($"{PanelType.FullName} destroy time: {c.Milliseconds}"));

        OnPanelDestroying();

        Object.Destroy(_panelGameObject);
        Panel = null;
        _panelGameObject = null;

        OnPanelDestroyed();
    }

    public virtual void ShowPanel() {
        if (Panel == null) return;

        Panel.Show(true);
        Panel.Focus();
    }

    public virtual void HidePanel() {
        if (Panel == null) return;

        Panel.Hide();
    }
}