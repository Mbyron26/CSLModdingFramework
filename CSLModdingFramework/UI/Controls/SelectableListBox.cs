using System;
using System.Collections.Generic;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class SelectableListBox : ScrollViewer {
    protected readonly List<SelectableItem> _items = new();
    protected readonly Dictionary<int, SelectableItem> _itemsById = new();
    protected IReadOnlyList<SelectableItem> _readOnlyItems;
    protected IReadOnlyDictionary<int, SelectableItem> _readOnlyItemsById;
    protected int _itemHeight = 30;

    public IReadOnlyList<SelectableItem> Items => _readOnlyItems ??= _items.AsReadOnlyList();
    public IReadOnlyDictionary<int, SelectableItem> ItemsById => _readOnlyItemsById ??= _itemsById.AsReadOnlyDictionary();

    public override ScrollOrientation Orientation {
        get => _orientation;
        set { }
    }

    public override ScrollSizePolicy SizeMode {
        get =>
            _sizeMode;
        set {
            if (_sizeMode == value) return;

            _sizeMode = value;
            RefreshContentLayout();
            UpdateSize();
        }
    }

    public int ItemHeight {
        get => _itemHeight;
        set {
            if (_itemHeight == value) return;

            _itemHeight = value;
            RefreshContentLayout();
        }
    }

    public LiteContainer ListBox { get; private set; }

    public SelectableItem AddItem(int id) {
        var item = CreateItem(id);
        RefreshContentLayout();
        return item;
    }

    public void AddItems(IEnumerable<int> ids) {
        foreach (var id in ids)
            CreateItem(id);

        RefreshContentLayout();
    }

    public bool TryGetItem(int id, out SelectableItem item) => _itemsById.TryGetValue(id, out item);

    private SelectableItem CreateItem(int id) {
        var item = ListBox.AddUIComponent<SelectableItem>();
        item.Id = id;
        item.size = new Vector2(width - _padding.Horizontal, _itemHeight);

        _items.Add(item);
        _itemsById.Add(id, item);

        return item;
    }

    public void DestroyItem(int id, Action<SelectableItem> onDestroying = null) {
        SuspendAutoLayout();

        if (_itemsById.TryGetValue(id, out var item)) {
            onDestroying?.Invoke(item);
            item.DestroySelf();
            _items.Remove(item);
            _itemsById.Remove(id);
        }

        ResumeAutoLayout();
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

    public override void Awake() {
        base.Awake();

        SuspendAutoLayout();

        SetOrientation(ScrollOrientation.Vertical);
        Padding = new EdgeInsets(0, 0, 4, 4);
        Orientation = ScrollOrientation.Vertical;
        VerticalScrollBarMargin = new EdgeInsets(0, 0, 4, 4);

        ListBox = SetContent<LiteContainer>();
        ListBox.Padding = new EdgeInsets(4, 4, 0, 0);
        ListBox.size = size;
        ListBox.AutoFitChildrenVertically = true;
        ListBox.AutoLayout = true;

        Background.Atlas = DefaultAtlas.Instance;
        Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
        Background.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlBackground);
        if (VerticalScrollBar != null) {
            VerticalScrollBar.width = 6;
            VerticalScrollBar.Background.Atlas = DefaultAtlas.Instance;
            VerticalScrollBar.Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius4);
            VerticalScrollBar.Background.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlBackgroundHovered);
            VerticalScrollBar.ThumbControl.Colors.SetValues(new Color32(96, 110, 130, 255));
        }

        ResumeAutoLayout();
    }

    public void RefreshContentLayout() {
        if (ListBox == null) return;

        if (_sizeMode == ScrollSizePolicy.Fixed) {
            ListBox.SetWidthIfChanged(width - _padding.Horizontal);
            foreach (var item in _items) {
                item.SetHeightIfChanged(_itemHeight);
            }

            return;
        }

        var visibleCount = 0;
        var itemsHeight = 0f;
        var maxItemWidth = 0f;

        foreach (var item in _items) {
            item.SetHeightIfChanged(_itemHeight);

            if (!item.isVisibleSelf) continue;

            visibleCount++;
            itemsHeight += item.height;
            maxItemWidth = Mathf.Max(maxItemWidth, item.MeasureRequiredWidth());
        }

        var contentPadding = ListBox.Padding;
        var popupPadding = Padding;
        var contentHeight = contentPadding.Vertical + itemsHeight + Mathf.Max(0, visibleCount - 1) * ListBox.RowGap;
        var willHaveScrollBar = contentHeight + popupPadding.Vertical > MaxFitSize.Height;
        var scrollBarWidth = VerticalScrollBar != null ? VerticalScrollBar.width : 0f;

        if (willHaveScrollBar) {
            var contentMinWidth = maxItemWidth + contentPadding.Horizontal;
            if (contentMinWidth <= MinFitSize.Width) {
                maxItemWidth = MinFitSize.Width - contentPadding.Horizontal - scrollBarWidth;
                ListBox.SetWidthIfChanged(MinFitSize.Width);
            }
            else if (contentMinWidth > MinFitSize.Width && contentMinWidth <= MaxFitSize.Width) {
                ListBox.SetWidthIfChanged(contentMinWidth + scrollBarWidth);
            }
            else {
                maxItemWidth = MaxFitSize.Width - contentPadding.Horizontal - scrollBarWidth;
                ListBox.SetWidthIfChanged(MaxFitSize.Width);
            }
        }
        else {
            var contentMinWidth = maxItemWidth + contentPadding.Horizontal;
            if (contentMinWidth <= MinFitSize.Width) {
                maxItemWidth = MinFitSize.Width - contentPadding.Horizontal;
                ListBox.SetWidthIfChanged(MinFitSize.Width);
            }
            else if (contentMinWidth > MinFitSize.Width && contentMinWidth <= MaxFitSize.Width) {
                ListBox.SetWidthIfChanged(contentMinWidth);
            }
            else {
                maxItemWidth = MaxFitSize.Width - contentPadding.Horizontal;
                ListBox.SetWidthIfChanged(MaxFitSize.Width);
            }
        }

        foreach (var item in _items) {
            item.width = maxItemWidth;
        }
    }
}