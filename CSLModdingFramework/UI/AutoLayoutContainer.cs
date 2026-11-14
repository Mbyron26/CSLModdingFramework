using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class AutoLayoutContainer : UIInteractiveControl, IAutoLayout {
    protected bool _autoLayout = true;
    protected int _autoLayoutSuspendCount;
    protected EdgeInsets _padding;

    public virtual bool AutoLayout {
        get => _autoLayout;
        set {
            if (value.Equals(_autoLayout)) return;

            _autoLayout = value;
            InvalidateAutoLayout();
        }
    }

    public virtual EdgeInsets Padding {
        get => _padding;
        set {
            if (value.EqualsApprox(_padding)) return;

            _padding = value;
            InvalidateAutoLayout();
        }
    }

    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public bool IsPerformingAutoLayout { get; protected set; }

    public abstract void PerformAutoLayout();

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    protected virtual void OnComponentAddedInvoke(UIComponent child) {
        if (!_autoLayout) return;

        AttachEvents(child);
        InvalidateAutoLayout();
    }

    protected virtual void OnComponentRemovedInvoke(UIComponent child) {
        if (child != null)
            DetachEvents(child);

        if (!_autoLayout) return;

        InvalidateAutoLayout();
    }

    protected override Plane[] GetClippingPlanes() => clipChildren ? GetPaddedClippingPlanes(_padding) : null;

    protected override void OnComponentAdded(UIComponent child) {
        OnComponentAddedInvoke(child);

        base.OnComponentAdded(child);
    }

    protected override void OnComponentRemoved(UIComponent child) {
        base.OnComponentRemoved(child);

        OnComponentRemovedInvoke(child);
    }

    private void AttachEvents(UIComponent child) {
        child.eventVisibilityChanged += OnChildVisibleChanged;
        child.eventPositionChanged += OnChildPositionChanged;
        child.eventSizeChanged += OnChildSizeChanged;
        child.eventZOrderChanged += OnChildZOrderChanged;
    }

    private void ChildInvalidatedLayout() {
        if (!_autoLayout) return;

        InvalidateAutoLayout();
    }

    private void DetachEvents(UIComponent child) {
        child.eventVisibilityChanged -= OnChildVisibleChanged;
        child.eventPositionChanged -= OnChildPositionChanged;
        child.eventSizeChanged -= OnChildPositionChanged;
        child.eventZOrderChanged -= OnChildZOrderChanged;
    }

    private void OnChildPositionChanged(UIComponent child, Vector2 value) => ChildInvalidatedLayout();
    private void OnChildSizeChanged(UIComponent component, Vector2 value) => ChildInvalidatedLayout();
    private void OnChildVisibleChanged(UIComponent child, bool value) => ChildInvalidatedLayout();
    private void OnChildZOrderChanged(UIComponent child, int value) => ChildInvalidatedLayout();
}