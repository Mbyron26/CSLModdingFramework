using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public class SegmentList : ItemListBase<SegmentItem, SegmentList> {
    protected BackgroundRenderer _backgroundRenderer;
    protected Orientation _direction;
    protected float _spacing;
    protected ThemeKind _theme;

    public SegmentList() => InvalidateOnStateChanged = true;

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public BackgroundRenderer BackgroundRenderer => _backgroundRenderer ??= new BackgroundRenderer(this);

    public Orientation Direction {
        get => _direction;
        set {
            if (value == _direction) return;

            _direction = value;
            InvalidateAutoLayout();
        }
    }

    public float Spacing {
        get => _spacing;
        set {
            if (Mathf.Approximately(value, _spacing)) return;

            _spacing = value;
            InvalidateAutoLayout();
        }
    }

    public virtual void SetTheme(ThemeKind theme) {
        if (_theme == theme) return;

        _theme = theme;
        var backgroundRender = BackgroundRenderer;
        backgroundRender.Atlas = DefaultAtlas.Instance;

        if (theme == ThemeKind.ClassicLight) {
            backgroundRender.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
            backgroundRender.Colors.SetValues(UIColorPalette.ClassicLightPalette.ElementNormal);
        }
        else {
            backgroundRender.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
            backgroundRender.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlBackground);
        }
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(100, 30);
    }

    public override void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        var visibleCount = 0;
        foreach (var item in _items) {
            if (item.isVisibleSelf)
                visibleCount++;
        }

        if (visibleCount == 0) {
            IsPerformingAutoLayout = false;
            return;
        }

        var contentWidth = width - _padding.Left - _padding.Right;
        var contentHeight = height - _padding.Top - _padding.Bottom;

        if (_direction == Orientation.Horizontal) {
            var totalSpacing = _spacing * (visibleCount - 1);
            var itemWidth = (contentWidth - totalSpacing) / visibleCount;
            var x = _padding.Left;
            foreach (var item in _items) {
                if (!item.isVisibleSelf) continue;

                item.relativePosition = new Vector2(x, _padding.Top);
                item.size = new Vector2(itemWidth, contentHeight);
                x += itemWidth + _spacing;
            }
        }
        else {
            var totalSpacing = _spacing * (visibleCount - 1);
            var itemHeight = (contentHeight - totalSpacing) / visibleCount;
            var y = _padding.Top;
            foreach (var item in _items) {
                if (!item.isVisibleSelf) continue;

                item.relativePosition = new Vector2(_padding.Left, y);
                item.size = new Vector2(contentWidth, itemHeight);
                y += itemHeight + _spacing;
            }
        }

        IsPerformingAutoLayout = false;
    }

    protected override void OnRebuildRenderData() => _backgroundRenderer?.Render();

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended)
            PerformAutoLayout();

        base.OnSizeChanged();
    }

    public SegmentItem AddItem(int id) {
        var item = AddUIComponent<SegmentItem>();
        item.Id = id;
        item.width = width - _padding.Horizontal;
        item.eventSizeChanged += OnItemSizeChanged;

        _items.Add(item);
        _itemsById.Add(id, item);

        InvalidateAutoLayout();
        return item;
    }

    public void DestroyItem(int id) {
        SuspendAutoLayout();

        if (_itemsById.TryGetValue(id, out var item)) {
            item.eventSizeChanged -= OnItemSizeChanged;
            item.eventVisibilityChanged -= OnItemVisibilityChanged;
            NotifyItemDestroying(item);
            _items.Remove(item);
            _itemsById.Remove(id);
            item.DestroySelf();
        }

        ResumeAutoLayout();
    }

    public void DestroyItems() {
        SuspendAutoLayout();

        foreach (var item in _items) {
            item.eventSizeChanged -= OnItemSizeChanged;
            item.eventVisibilityChanged -= OnItemVisibilityChanged;
            NotifyItemDestroying(item);
            item.DestroySelf();
        }

        _items.Clear();
        _itemsById.Clear();

        ResumeAutoLayout();
    }

    private void OnItemSizeChanged(UIComponent component, Vector2 value) => InvalidateAutoLayout();

    private void OnItemVisibilityChanged(UIComponent component, bool value) => InvalidateAutoLayout();
}