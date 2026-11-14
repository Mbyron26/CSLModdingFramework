using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class SpriteView : UIInteractiveControl {
    protected IAtlas _atlas;
    protected string _spriteName;
    protected UISpriteFlip _flip;
    protected UIFillDirection _fillDirection;
    protected float _fillAmount = 1f;
    protected bool _invertFill;
    protected EdgeInsets _padding;
    protected UIHorizontalAlignment _horizontalAlignment;
    protected UIVerticalAlignment _verticalAlignment;

    public event UIEventHandler<SpriteView, string> SpriteNameChanged;

    public ColorStateValue Colors { get; protected set; }

    public virtual UIHorizontalAlignment HorizontalAlignment {
        get => _horizontalAlignment;
        set {
            if (value == _horizontalAlignment) return;

            _horizontalAlignment = value;
            Invalidate();
        }
    }

    public virtual UIVerticalAlignment VerticalAlignment {
        get => _verticalAlignment;
        set {
            if (value == _verticalAlignment) return;

            _verticalAlignment = value;
            Invalidate();
        }
    }

    public EdgeInsets Padding {
        get => _padding;
        set {
            if (value.Equals(_padding)) return;

            _padding = value;
            Invalidate();
        }
    }

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
            OnSpriteNameChanged(value);
        }
    }

    public UISpriteFlip Flip {
        get => _flip;
        set {
            if (value == _flip) return;

            _flip = value;
            Invalidate();
        }
    }

    public UIFillDirection FillDirection {
        get => _fillDirection;
        set {
            if (value == _fillDirection) return;

            _fillDirection = value;
            Invalidate();
        }
    }

    public virtual float FillAmount {
        get => _fillAmount;
        set {
            if (Mathf.Approximately(value, _fillAmount)) return;

            _fillAmount = Mathf.Max(0.0f, Mathf.Min(1f, value));
            Invalidate();
        }
    }

    public bool InvertFill {
        get => _invertFill;
        set {
            if (value == _invertFill) return;

            _invertFill = value;
            Invalidate();
        }
    }

    protected virtual void OnSpriteNameChanged(string value) => SpriteNameChanged?.Invoke(this, value);

    protected virtual Vector3 GetRenderOffset() {
        var result = pivot.GetUpperLeftOffset(size, arbitraryPivotOffset);
        result += LayoutUtility.CalculateAlignedOffset(_horizontalAlignment, _verticalAlignment, m_Size, m_Size, _padding);
        return result;
    }

    public override void Awake() {
        base.Awake();

        Colors = new ColorStateValue(this);
    }

    public override Vector2 CalculateMinimumSize() {
        var zero = Vector2.zero;

        if (_atlas == null || string.IsNullOrEmpty(_spriteName) || !_atlas.TryGetSprite(_spriteName, out int spriteIndex)) return zero;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

        var rect = spriteMetadata.Region;

        return rect is { width: > 0, height: > 0 } ? Vector2.Max(base.CalculateMinimumSize(), new Vector2(rect.width, rect.height)) : base.CalculateMinimumSize();
    }

    protected override void OnRebuildRenderData() {
        if (_atlas == null || _atlas.Material == null || !isVisible) return;

        renderData.material = _atlas.Material;

        if (string.IsNullOrEmpty(_spriteName)) return;

        if (!_atlas.TryGetSprite(_spriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = Colors.GetValue(_state),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = GetRenderOffset(),
            UnitsPerPixel = UnitsPerPixel,
            Size = size,
            SpriteMetadata = spriteMetadata,
            InvertFill = _invertFill
        };

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }
}