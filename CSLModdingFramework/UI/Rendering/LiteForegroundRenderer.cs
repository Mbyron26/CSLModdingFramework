using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public class LiteForegroundRenderer : ForegroundRendererBase<UIControl> {
    private string _sprite;
    private Color32 _normalColor = UIColorPalette.White;
    private Color32 _disabledColor = UIColorPalette.White;

    public string Sprite {
        get => _sprite;
        set {
            if (_sprite == value) return;

            _sprite = value;
            InvalidateAutoRender();
        }
    }

    public Color32 NormalColor {
        get => _normalColor;
        set {
            if (_normalColor.EqualsColor(value)) return;

            _normalColor = value;
            InvalidateAutoRender();
        }
    }

    public Color32 DisabledColor {
        get => _disabledColor;
        set {
            if (_disabledColor.EqualsColor(value)) return;

            _disabledColor = value;
            InvalidateAutoRender();
        }
    }

    public LiteForegroundRenderer(UIControl owner) : base(owner) { }

    protected override string GetSprite() => _sprite;
    protected override Color32 GetColor() => Owner.isEnabled ? _normalColor : _disabledColor;
}