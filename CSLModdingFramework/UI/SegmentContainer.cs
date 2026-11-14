using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public class SegmentContainer : UIInteractiveControl, IThemeable, IAutoLayout {
    protected BackgroundRenderer _backgroundRenderer;
    protected EdgeInsets _padding;
    protected Orientation _direction;
    protected List<SegmentItem> _items;
    protected Dictionary<int, SegmentItem> _itemsById;
    protected float _spacing;
    protected int _autoLayoutSuspendCount;
    protected IReadOnlyList<SegmentItem> _readOnlyItems;
    protected IReadOnlyDictionary<int, SegmentItem> _readOnlyItemsById;
    protected ThemeKind _theme;

    public BackgroundRenderer BackgroundRenderer => _backgroundRenderer ??= new BackgroundRenderer(this);
    public IReadOnlyList<SegmentItem> Items => _readOnlyItems ??= _items.AsReadOnlyList();
    public IReadOnlyDictionary<int, SegmentItem> ItemsById => _readOnlyItemsById ??= _itemsById.AsReadOnlyDictionary();
    public bool IsPerformingAutoLayout { get; protected set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public float Spacing {
        get => _spacing;
        set {
            if (Mathf.Approximately(value, _spacing)) return;

            _spacing = value;
            InvalidateAutoLayout();
        }
    }

    public Orientation Direction {
        get => _direction;
        set {
            if (value == _direction) return;

            _direction = value;
            InvalidateAutoLayout();
        }
    }

    public EdgeInsets Padding {
        get => _padding;
        set {
            if (value == _padding) return;

            _padding = value;
            InvalidateAutoLayout();
        }
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    public virtual void PerformAutoLayout() {
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

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(100, 30);
        _items = new List<SegmentItem>();
        _itemsById = new Dictionary<int, SegmentItem>();
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended)
            PerformAutoLayout();

        base.OnSizeChanged();
    }

    protected override void OnRebuildRenderData() => _backgroundRenderer?.Render();

    public SegmentItem AddItem(int id, Action<SegmentItem> onCreated = null) {
        var item = AddUIComponent<SegmentItem>();
        item.Id = id;
        item.width = width - _padding.Horizontal;
        item.eventSizeChanged += OnItemSizeChanged;
        onCreated?.Invoke(item);

        _items.Add(item);
        _itemsById.Add(id, item);

        InvalidateAutoLayout();
        return item;
    }

    public void DestroyItems() {
        SuspendAutoLayout();

        foreach (var item in _items) {
            item.DestroySelf();
        }

        _items.Clear();
        _itemsById.Clear();

        ResumeAutoLayout();
    }

    private void OnItemSizeChanged(UIComponent component, Vector2 value) => InvalidateAutoLayout();
}