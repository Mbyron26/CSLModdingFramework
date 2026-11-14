using System.Collections.Generic;
using ColossalFramework.UI;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class ItemListBase<TItem, TList> : UIInteractiveControl, IAutoLayout where TItem : UIComponent where TList : ItemListBase<TItem, TList> {
    protected readonly List<TItem> _items = new();
    protected readonly Dictionary<int, TItem> _itemsById = new();
    protected EdgeInsets _padding;
    protected float _itemGap = 8f;
    protected int _autoLayoutSuspendCount;
    protected IReadOnlyList<TItem> _readOnlyItems;
    protected IReadOnlyDictionary<int, TItem> _readOnlyItemsById;

    public event UIEventHandler<TItem> ItemCreated;
    public event UIEventHandler<TItem> ItemDestroying;

    public ItemListBase() => InvalidateOnStateChanged = false;

    public bool IsPerformingAutoLayout { get; protected set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;
    public IReadOnlyList<TItem> Items => _readOnlyItems ??= _items.AsReadOnlyList();
    public IReadOnlyDictionary<int, TItem> ItemsById => _readOnlyItemsById ??= _itemsById.AsReadOnlyDictionary();

    public virtual float ItemGap {
        get => _itemGap;
        set {
            if (Mathf.Approximately(_itemGap, value)) return;

            _itemGap = value;
            InvalidateAutoLayout();
        }
    }

    public virtual EdgeInsets Padding {
        get => _padding;
        set {
            if (_padding.Equals(value)) return;

            _padding = value;
            InvalidateAutoLayout();
        }
    }

    public abstract void PerformAutoLayout();

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    public bool TryGetItem(int id, out TItem item) => _itemsById.TryGetValue(id, out item);

    protected void NotifyItemCreated(TItem item) => ItemCreated?.Invoke(item);
    protected void NotifyItemDestroying(TItem item) => ItemDestroying?.Invoke(item);
}