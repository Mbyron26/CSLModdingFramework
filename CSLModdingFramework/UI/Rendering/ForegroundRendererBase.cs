using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public abstract class ForegroundRendererBase<T> : ILayerRenderer<T>, IAutoRender where T : UIControl {
    protected IAtlas _atlas;
    protected int _autoRenderSuspendCount;
    protected Vector2 _customSize;
    protected UIHorizontalAlignment _horizontalAlignment = UIHorizontalAlignment.Center;
    protected bool _isEnabled = true;
    protected EdgeInsets _padding;
    protected UIRenderData _renderData;
    protected float _scale = 1f;
    protected SpriteScaleMode _scaleMode = SpriteScaleMode.Contain;
    protected UIVerticalAlignment _verticalAlignment = UIVerticalAlignment.Middle;
    public ForegroundRendererBase(T owner) => Owner = owner;

    public IAtlas Atlas {
        get => _atlas;
        set {
            if (Equals(value, _atlas)) return;

            _atlas = value;
            InvalidateAutoRender();
        }
    }

    public Vector2 CustomSize {
        get => _customSize;
        set {
            if (value == _customSize) return;

            _customSize = value;
            InvalidateAutoRender();
        }
    }

    public UIHorizontalAlignment HorizontalAlignment {
        get => _horizontalAlignment;
        set {
            if (value == _horizontalAlignment) return;

            _horizontalAlignment = value;
            InvalidateAutoRender();
        }
    }

    public bool IsAutoRenderSuspended => _autoRenderSuspendCount > 0;

    public bool IsEnabled {
        get => _isEnabled;
        set {
            if (value.Equals(_isEnabled)) return;

            _isEnabled = value;
            InvalidateAutoRender();
        }
    }

    public bool IsPerformingAutoRender { get; protected set; }

    public T Owner { get; }

    public EdgeInsets Padding {
        get => _padding;
        set {
            if (value.Equals(_padding)) return;

            _padding = value;
            InvalidateAutoRender();
        }
    }

    public float Scale {
        get => _scale;
        set {
            if (Mathf.Approximately(value, _scale)) return;

            _scale = value;
            InvalidateAutoRender();
        }
    }

    public SpriteScaleMode ScaleMode {
        get => _scaleMode;
        set {
            if (value == _scaleMode) return;

            _scaleMode = value;
            InvalidateAutoRender();
        }
    }

    public UIVerticalAlignment VerticalAlignment {
        get => _verticalAlignment;
        set {
            if (value == _verticalAlignment) return;

            _verticalAlignment = value;
            InvalidateAutoRender();
        }
    }

    protected abstract Color32 GetColor();

    protected abstract string GetSprite();

    public virtual Vector3 GetRenderOffset(Vector2 renderSize) {
        var ownerSize = Owner.size;
        var result = Owner.pivot.TransformToUpperLeft(ownerSize, Owner.arbitraryPivotOffset);
        result += LayoutUtility.CalculateAlignedOffset(_horizontalAlignment, _verticalAlignment, ownerSize, renderSize, _padding);

        return result;
    }

    public virtual Vector2 GetRenderSize() {
        if (_atlas == null || !_atlas.TryGetSprite(GetSprite(), out int spriteIndex)) return Vector2.zero;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);
        return GetRenderSize(spriteMetadata);
    }

    public virtual void PerformAutoRender() {
        if (IsPerformingAutoRender) return;

        IsPerformingAutoRender = true;
        Owner?.Invalidate();
        IsPerformingAutoRender = false;
    }

    public virtual void Render() {
        if (IsAutoRenderSuspended || _atlas == null) return;

        if (_renderData is not null) {
            _renderData.Clear();
        }
        else {
            _renderData = UIRenderData.Obtain();
            Owner.RenderData.Add(_renderData);
        }

        if (!_isEnabled) return;

        _renderData.material = _atlas.Material;

        if (!_atlas.TryGetSprite(GetSprite(), out int spriteIndex))
            return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

        var renderSize = GetRenderSize(spriteMetadata);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = GetColor(),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = GetRenderOffset(renderSize),
            UnitsPerPixel = Owner.UnitsPerPixel,
            Size = renderSize,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_renderData, spriteRenderData);
    }

    public virtual void ResumeAutoRender(bool performNow = true) {
        if (_autoRenderSuspendCount == 0) return;

        _autoRenderSuspendCount--;

        if (_autoRenderSuspendCount != 0 || !performNow) return;

        PerformAutoRender();
    }

    public virtual void SuspendAutoRender() => _autoRenderSuspendCount++;

    protected virtual Vector2 GetRenderSize(AtlasSpriteMetadata atlasSpriteMetadata) {
        var ownerSize = Owner.size;
        var ownerWidth = ownerSize.x;
        var ownerHeight = ownerSize.y;

        Vector2 vector;
        float ratio;

        var pixelSize = atlasSpriteMetadata.PixelSize;

        switch (_scaleMode) {
            case SpriteScaleMode.OriginalSize:
                vector = pixelSize;
                break;

            case SpriteScaleMode.FixedSize:
                vector = _customSize * _scale;
                break;

            case SpriteScaleMode.Stretch:
                vector = ownerSize - new Vector2(_padding.Horizontal, _padding.Vertical);
                break;

            case SpriteScaleMode.Contain:
                ratio = Mathf.Min(ownerWidth / pixelSize.x, ownerHeight / pixelSize.y);
                vector = new Vector2(pixelSize.x * ratio, pixelSize.y * ratio);
                break;

            case SpriteScaleMode.ContainOverflow:
                ratio = Mathf.Min(ownerWidth / pixelSize.x, ownerHeight / pixelSize.y);
                vector = new Vector2(pixelSize.x * ratio, pixelSize.y * ratio) * _scale;
                break;

            case SpriteScaleMode.Cover:
                ratio = Mathf.Max(ownerWidth / pixelSize.x, ownerHeight / pixelSize.y);
                vector = new Vector2(pixelSize.x * ratio, pixelSize.y * ratio);
                break;

            case SpriteScaleMode.FitWidth:
                ratio = ownerWidth / pixelSize.x;
                vector = new Vector2(ownerWidth, pixelSize.y * ratio);
                break;

            case SpriteScaleMode.FitHeight:
                ratio = ownerHeight / pixelSize.y;
                vector = new Vector2(pixelSize.x * ratio, ownerHeight);
                break;

            case SpriteScaleMode.Disabled:
            default:
                vector = Vector2.zero;
                break;
        }

        return vector;
    }

    protected virtual void InvalidateAutoRender() {
        if (IsAutoRenderSuspended) return;

        Owner?.Invalidate();
    }
}