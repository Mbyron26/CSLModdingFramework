using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public abstract class BackgroundRendererBase<T> : ILayerRenderer<T>, IAutoLayout where T : UIControl {
    protected IAtlas _atlas;
    protected int _autoLayoutSuspendCount;
    protected bool _isEnabled = true;
    protected EdgeInsets _padding;
    protected UIRenderData _renderData;

    public BackgroundRendererBase(T owner) => Owner = owner;

    public IAtlas Atlas {
        get => _atlas;
        set {
            if (Equals(value, _atlas)) return;

            _atlas = value;
            InvalidateAutoLayout();
        }
    }

    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public bool IsEnabled {
        get => _isEnabled;
        set {
            if (value.Equals(_isEnabled)) return;

            _isEnabled = value;
            InvalidateAutoLayout();
        }
    }

    public bool IsPerformingAutoLayout { get; protected set; }

    public T Owner { get; }

    public EdgeInsets Padding {
        get => _padding;
        set {
            if (value.Equals(_padding)) return;

            _padding = value;
            InvalidateAutoLayout();
        }
    }

    protected abstract Color32 GetColor();

    protected abstract string GetSprite();

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        IsPerformingAutoLayout = false;
    }

    public virtual void Render() {
        if (IsAutoLayoutSuspended || _atlas == null) return;

        if (_renderData is not null) {
            _renderData.Clear();
        }
        else {
            _renderData = UIRenderData.Obtain();
            Owner.RenderData.Add(_renderData);
        }

        if (!IsEnabled) return;

        _renderData.material = _atlas.Material;

        if (!_atlas.TryGetSprite(GetSprite(), out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

        var size = Owner.size;

        var paddedSize = new Vector2(
            size.x - _padding.Horizontal,
            size.y - _padding.Vertical
        );

        var offset = Owner.pivot.GetUpperLeftOffset(size, Owner.arbitraryPivotOffset);
        offset.x += _padding.Left;
        offset.y += _padding.Top;

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = GetColor(),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = offset,
            UnitsPerPixel = Owner.UnitsPerPixel,
            Size = paddedSize,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_renderData, spriteRenderData);
    }

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Owner?.Invalidate();
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Owner?.Invalidate();
    }
}