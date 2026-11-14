using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public sealed class RadioButtonList : ItemListBase<RadioButton, RadioButtonList> {
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

    public RadioButton AddItem(int id) {
        var item = AddUIComponent<RadioButton>();
        item.Id = id;
        item.width = width - _padding.Horizontal;
        item.eventSizeChanged += OnItemSizeChanged;
        item.eventVisibilityChanged += OnItemVisibilityChanged;
        NotifyItemCreated(item);

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
            item.DestroySelf();
        }

        _items.Clear();
        _itemsById.Clear();

        ResumeAutoLayout();
    }

    private void OnItemSizeChanged(UIComponent component, Vector2 value) => InvalidateAutoLayout();

    private void OnItemVisibilityChanged(UIComponent component, bool value) => InvalidateAutoLayout();
}