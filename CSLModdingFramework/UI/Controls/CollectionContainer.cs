using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class CollectionContainer : UIInteractiveControl, IAutoLayout {
    protected int _autoLayoutSuspendCount;
    protected BackgroundRenderer _background;
    protected float _columnGap;
    protected ForegroundRenderer _foreground;
    protected EdgeInsets _padding;
    protected float _rowGap;

    public virtual float ColumnGap {
        get => _columnGap;
        set {
            if (Mathf.Approximately(value, _columnGap)) return;

            _columnGap = value;
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

    public virtual float RowGap {
        get => _rowGap;
        set {
            if (Mathf.Approximately(value, _rowGap)) return;

            _rowGap = value;
            InvalidateAutoLayout();
        }
    }

    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);
    public ForegroundRenderer Foreground => _foreground ??= new ForegroundRenderer(this);

    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public bool IsPerformingAutoLayout { get; protected set; }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(200, 200);
    }

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        try {
            var containerWidth = m_Size.x;
            var left = _padding.Left;
            var right = containerWidth - _padding.Right;

            var x = left;
            var y = _padding.Top;

            var currentRowHeight = 0f;

            for (var i = 0; i < components.Count; i++) {
                var child = components[i];

                if (child == null ||
                    string.Equals(child.name, ExcludedControl) ||
                    !child.isVisibleSelf)
                    continue;

                var childWidth = child.width;
                var childHeight = child.height;

                var shouldWrap = x > left && x + childWidth > right;
                if (shouldWrap) {
                    x = left;
                    y += currentRowHeight + _rowGap;
                    currentRowHeight = 0f;
                }

                child.relativePosition = new Vector3(x, y);
                x += childWidth + _columnGap;
                if (childHeight > currentRowHeight)
                    currentRowHeight = childHeight;
            }

            var totalHeight = y + currentRowHeight + _padding.Bottom;

            SetHeightIfChanged(totalHeight);
        }
        finally {
            IsPerformingAutoLayout = false;
        }
    }

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

    protected override Plane[] GetClippingPlanes() => clipChildren ? GetPaddedClippingPlanes(_padding) : null;

    protected override void OnComponentAdded(UIComponent child) {
        base.OnComponentAdded(child);

        AttachEvents(child);
        InvalidateAutoLayout();
    }

    protected override void OnComponentRemoved(UIComponent child) {
        base.OnComponentRemoved(child);

        if (child != null)
            DetachEvents(child);

        InvalidateAutoLayout();
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
    }

    private void AttachEvents(UIComponent child) => child.eventVisibilityChanged += OnChildVisibleChanged;

    private void DetachEvents(UIComponent child) => child.eventVisibilityChanged -= OnChildVisibleChanged;

    private void OnChildVisibleChanged(UIComponent child, bool value) => InvalidateAutoLayout();
}