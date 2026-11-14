using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class PlainOptionListBox : ItemListBase<SelectableItem,PlainOptionListBox> {
    protected int _itemHeight = 30;

    public event Action<SelectableItem> ItemAdded;
    public event Action<IList<SelectableItem>> ItemsAdded;

    public int ItemHeight {
        get => _itemHeight;
        set {
            if (_itemHeight == value) return;

            _itemHeight = value;
            InvalidateAutoLayout();
        }
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(200, 32);
    }

    public override void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        var itemWidth = width - _padding.Horizontal;

        var itemXOffset = _padding.Left;
        var itemYOffset = _padding.Top;
        var itemsCount = _items.Count;

        for (var i = 0; i < itemsCount; i++) {
            var item = _items[i];
            if (!item.isVisibleSelf) continue;

            item.SetWidthIfChanged(itemWidth);
            item.relativePosition = new Vector3(itemXOffset, itemYOffset, 0);

            itemYOffset += item.height;
            if (i != itemsCount - 1) {
                itemYOffset += _itemGap;
            }
        }

        SetHeightIfChanged(itemYOffset + _padding.Bottom);

        IsPerformingAutoLayout = false;
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended)
            PerformAutoLayout();

        base.OnSizeChanged();
    }

    public SelectableItem AddItem(int id) {
        var item = CreateItem(id);
        ItemAdded?.Invoke(item);
        InvalidateAutoLayout();
        return item;
    }

    public void AddItems(IEnumerable<int> ids) {
        var items = new List<SelectableItem>();

        foreach (var id in ids)
            items.Add(CreateItem(id));

        if (items.Count > 0)
            ItemsAdded?.Invoke(items);

        InvalidateAutoLayout();
    }

    private SelectableItem CreateItem(int id) {
        var item = AddUIComponent<SelectableItem>();
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
}