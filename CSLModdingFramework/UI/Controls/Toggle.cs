using System;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class Toggle : UIInteractiveControl, IThemeable {
    protected bool _isOn;
    protected ThemeKind _theme;
    protected ToggleBackgroundRenderer _background;
    protected ToggleForegroundRenderer _foreground;

    public event UIEventHandler<Toggle, bool> ToggleChanged;
    public event Action<bool> IsOnChanged;

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public ToggleBackgroundRenderer Background => _background ??= new ToggleBackgroundRenderer(this);
    public ToggleForegroundRenderer Foreground => _foreground ??= new ToggleForegroundRenderer(this);
    public bool ChangeToggleStateOnClicked { get; set; } = true;

    public bool IsOn {
        get => _isOn;
        set => SetToggleState(value);
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(40, 24);
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
    }

    protected override void OnClick(UIMouseEventParameter p) {
        if (ChangeToggleStateOnClicked) {
            SetToggleState(!_isOn);
        }

        base.OnClick(p);
    }

    public void SetTheme(ThemeKind theme) {
        if (theme == _theme) return;

        _theme = theme;
        var bgRenderer = Background;
        bgRenderer.SuspendAutoLayout();
        bgRenderer.Atlas = DefaultAtlas.Instance;
        bgRenderer.OffSprites.SuspendAutoRender();
        bgRenderer.OnSprites.SuspendAutoRender();
        bgRenderer.OffSprites.SetValues(DefaultAtlas.SmoothRectangleRadius12);
        bgRenderer.OnSprites.SetValues(DefaultAtlas.SmoothRectangleRadius12);
        bgRenderer.OffColors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
        bgRenderer.OnColors.SetValues(UIColorPalette.GreenColors);
        bgRenderer.OffSprites.ResumeAutoRender();
        bgRenderer.OnSprites.ResumeAutoRender();
        bgRenderer.ResumeAutoLayout();

        var fgRenderer = Foreground;
        fgRenderer.SuspendAutoRender();
        fgRenderer.CustomSize = size;
        fgRenderer.ScaleMode = SpriteScaleMode.FixedSize;
        fgRenderer.Atlas = DefaultAtlas.Instance;
        fgRenderer.OnColors.SuspendAutoRender();
        fgRenderer.OffColors.SuspendAutoRender();
        fgRenderer.OnColors.SetValues(UIColorPalette.ToggleForegroundNormal);
        fgRenderer.OffColors.SetValues(UIColorPalette.ToggleForegroundNormal);
        fgRenderer.OnColors.Disabled = UIColorPalette.ToggleForegroundDisabled;
        fgRenderer.OffColors.Disabled = UIColorPalette.ToggleForegroundDisabled;
        fgRenderer.OnSprites.SetValues(DefaultAtlas.ToggleOnFg);
        fgRenderer.OffSprites.SetValues(DefaultAtlas.ToggleOffFg);
        fgRenderer.OnColors.ResumeAutoRender();
        fgRenderer.OffColors.ResumeAutoRender();
        fgRenderer.ResumeAutoRender();
    }

    public bool SetToggleStateWithoutNotify(bool isOn) => SetToggleState(isOn, false);

    public bool SetToggleState(bool isOn, bool notify = true) {
        if (!isEnabled || isOn == _isOn) return false;

        _isOn = isOn;
        Invalidate();

        if (notify) {
            ToggleChanged?.Invoke(this, IsOn);
            IsOnChanged?.Invoke(IsOn);
        }

        return true;
    }
}