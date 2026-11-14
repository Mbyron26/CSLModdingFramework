using System;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI;

public sealed class CheckboxList : ItemListBase<Checkbox, CheckboxList> {
    public Checkbox SelectAllItem { get; private set; }

    public override void Awake() {
        base.Awake();

        size = new Vector2(200, 32);
    }

    public override void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        var itemWidth = width - _padding.Horizontal;

        var itemXOffset = _padding.Left;
        var itemYOffset = _padding.Top;
        var itemsCount = _items.Count;

        if (SelectAllItem != null && SelectAllItem.isVisibleSelf) {
            SelectAllItem.SetWidthIfChanged(itemWidth);
            SelectAllItem.relativePosition = new Vector3(itemXOffset, itemYOffset, 0);

            itemYOffset += SelectAllItem.height;
            if (itemsCount > 0) {
                itemYOffset += _itemGap;
            }
        }

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

    public Checkbox AddItem(int id) => AddItemInternal(id);

    public Checkbox AddSelectAllItem() => SelectAllItem != null ? SelectAllItem : AddItemInternal(-1, true);

    public void DestroyItems() {
        SuspendAutoLayout();

        foreach (var item in _items) {
            item.DestroySelf();
        }

        _items.Clear();
        _itemsById.Clear();

        ResumeAutoLayout();
    }

    public void DestroyItem(int id, Action<Checkbox> onDestroying = null) {
        SuspendAutoLayout();

        if (_itemsById.TryGetValue(id, out var item)) {
            onDestroying?.Invoke(item);
            item.DestroySelf();
            _items.Remove(item);
            _itemsById.Remove(id);
        }

        ResumeAutoLayout();
    }

    private Checkbox AddItemInternal(int id, bool isMaster = false) {
        var item = AddUIComponent<Checkbox>();
        item.Id = id;
        item.width = width - _padding.Horizontal;
        item.eventSizeChanged += OnItemSizeChanged;
        NotifyItemCreated(item);

        if (!isMaster) {
            _items.Add(item);
            _itemsById.Add(id, item);
        }
        else {
            SelectAllItem = item;
        }

        InvalidateAutoLayout();
        return item;
    }

    private void OnItemSizeChanged(UIComponent component, Vector2 value) => InvalidateAutoLayout();
}