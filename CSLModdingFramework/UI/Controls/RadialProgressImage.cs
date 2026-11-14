using ColossalFramework;
using ColossalFramework.UI;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.UI.Animations;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class RadialProgressImage : RadialSpriteView, IThemeable {
    protected UIRenderData _backgroundRenderData;
    protected string _backgroundSpriteName;
    protected ThemeKind _theme;
    protected Color32 _backgroundNormalColor = new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    protected Color32 _backgroundDisabledColor = new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    private LiteFloatAnimation _fillAnim;
    private bool _useAnimation;

    public virtual ThemeKind Theme {
        get => _theme;
        set {
            if (value == _theme) return;

            _theme = value;
            Atlas = DefaultAtlas.Instance;
            SpriteName = DefaultAtlas.RadioIndicator;
            BackgroundSpriteName = DefaultAtlas.RadioIndicator;
            if (_theme == ThemeKind.ClassicLight) {
                BackgroundNormalColor = UIColorPalette.ClassicLightPalette.ElementNormal;
                BackgroundDisabledColor = UIColorPalette.ClassicLightPalette.ElementDisabled;
            }
            else {
                BackgroundNormalColor = UIColorPalette.ClassicDarkPalette.SettingItemControlBackground;
                BackgroundDisabledColor = UIColorPalette.ClassicDarkPalette.SettingItemControlBackgroundDisabled;
            }
        }
    }

    public override float FillAmount {
        get => _fillAmount;
        set {
            if (Mathf.Approximately(value, _fillAmount)) return;
            _useAnimation = false;
            _fillAnim.JumpTo(value);
            _fillAmount = Mathf.Max(0.0f, Mathf.Min(1f, value));
            Invalidate();
        }
    }

    public string BackgroundSpriteName {
        get => _backgroundSpriteName;
        set {
            if (string.Equals(value, _backgroundSpriteName)) return;

            _backgroundSpriteName = value;
            Invalidate();
            OnSpriteNameChanged(value);
        }
    }

    public Color32 BackgroundNormalColor {
        get => _backgroundNormalColor;
        set {
            if (value.EqualsColor(_backgroundNormalColor)) return;

            _backgroundNormalColor = value;
            Invalidate();
        }
    }

    public Color32 BackgroundDisabledColor {
        get => _backgroundDisabledColor;
        set {
            if (value.EqualsColor(_backgroundDisabledColor)) return;

            _backgroundDisabledColor = value;
            Invalidate();
        }
    }

    public virtual void RenderBackground() {
        if (_atlas == null) return;

        if (_backgroundRenderData is not null) {
            _backgroundRenderData.Clear();
        }
        else {
            _backgroundRenderData = UIRenderData.Obtain();
            RenderData.Add(_backgroundRenderData);
        }

        _backgroundRenderData.material = _atlas.Material;

        if (!_atlas.TryGetSprite(_backgroundSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = isEnabled ? _backgroundNormalColor : _backgroundDisabledColor,
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = Vector3.zero,
            UnitsPerPixel = UnitsPerPixel,
            Size = m_Size,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_backgroundRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_backgroundRenderData, spriteRenderData);
    }

    public override void Update() {
        base.Update();

        if (!_useAnimation) return;
        _fillAnim.Update(Time.deltaTime);
        _fillAmount = _fillAnim.Value;
        Invalidate();
        if (!_fillAnim.IsRunning) _useAnimation = false;
    }

    protected override void OnRebuildRenderData() {
        RenderBackground();

        base.OnRebuildRenderData();
    }

    public void StartDefaultFill() => StartFill(0f, 1f, 5, EasingType.Linear);

    public void StartFill(float from, float to, float duration, EasingType easing) {
        _fillAnim.Start(from, to, duration, easing);
        _useAnimation = true;
    }
}