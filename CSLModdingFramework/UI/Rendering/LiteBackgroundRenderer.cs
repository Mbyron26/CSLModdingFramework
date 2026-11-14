using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public sealed class LiteBackgroundRenderer : BackgroundRendererBase<UIControl> {
    private string _spriteName;
    private Color32 _normalColor = UIColorPalette.White;
    private Color32 _disabledColor = UIColorPalette.White;

    public string SpriteName {
        get => _spriteName;
        set {
            if (string.Equals(value, _spriteName)) return;

            _spriteName = value;
            InvalidateAutoLayout();
        }
    }

    public Color32 NormalColor {
        get => _normalColor;
        set {
            if (_normalColor.EqualsColor(value)) return;

            _normalColor = value;
            InvalidateAutoLayout();
        }
    }

    public Color32 DisabledColor {
        get => _disabledColor;
        set {
            if (_disabledColor.EqualsColor(value)) return;

            _disabledColor = value;
            InvalidateAutoLayout();
        }
    }

    public LiteBackgroundRenderer(UIControl owner) : base(owner) { }

    protected override string GetSprite() => _spriteName;
    protected override Color32 GetColor() => Owner.isEnabled ? _normalColor : _disabledColor;
}