using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class Divider : UIControl {
    private IAtlas _atlas = DefaultAtlas.Instance;
    private string _spriteName = DefaultAtlas.Rectangle;
    private Color32 _normalColor = UIColorPalette.White;
    private Color32 _disabledColor = UIColorPalette.White;

    public IAtlas Atlas {
        get => _atlas;
        set {
            if (Equals(value, _atlas)) return;

            _atlas = value;
            Invalidate();
        }
    }

    public string SpriteName {
        get => _spriteName;
        set {
            if (string.Equals(value, _spriteName)) return;

            _spriteName = value;
            Invalidate();
        }
    }

    public Color32 NormalColor {
        get => _normalColor;
        set {
            if (_normalColor.EqualsColor(value)) return;

            _normalColor = value;
            Invalidate();
        }
    }

    public Color32 DisabledColor {
        get => _disabledColor;
        set {
            if (_disabledColor.EqualsColor(value)) return;

            _disabledColor = value;
            Invalidate();
        }
    }

    protected override void OnRebuildRenderData() {
        if (_atlas == null) return;

        renderData.material = _atlas.Material;
        if (!_atlas.TryGetSprite(_spriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);
        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = isEnabled ? _normalColor : _disabledColor,
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
            UnitsPerPixel = UnitsPerPixel,
            Size = size,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }
}