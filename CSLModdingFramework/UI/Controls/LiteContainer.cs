using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class LiteContainer : AutoLayoutContainer {
    protected float _columnGap;
    protected float _rowGap;
    protected FlexDirection _direction = FlexDirection.Column;
    protected FlexAlignItems _alignItems;
    protected bool _autoFitChildrenVertically;
    protected bool _autoFitChildrenHorizontally;
    protected BackgroundRenderer _background;
    protected ForegroundRenderer _foreground;

    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);
    public ForegroundRenderer Foreground => _foreground ??= new ForegroundRenderer(this);

    public virtual float ColumnGap {
        get => _columnGap;
        set {
            if (Mathf.Approximately(value, _columnGap)) return;

            _columnGap = value;
            InvalidateAutoLayout();
        }
    }

    public virtual FlexDirection Direction {
        get => _direction;
        set {
            if (value.Equals(_direction)) return;

            _direction = value;
            InvalidateAutoLayout();
        }
    }

    public virtual FlexAlignItems AlignItems {
        get => _alignItems;
        set {
            if (value.Equals(_alignItems)) return;

            _alignItems = value;
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

    public bool AutoFitChildrenVertically {
        get => _autoFitChildrenVertically;
        set {
            if (value == _autoFitChildrenVertically) return;

            _autoFitChildrenVertically = value;
            InvalidateAutoLayout();
        }
    }

    public bool AutoFitChildrenHorizontally {
        get => _autoFitChildrenHorizontally;
        set {
            if (value == _autoFitChildrenHorizontally) return;

            _autoFitChildrenHorizontally = value;
            InvalidateAutoLayout();
        }
    }

    public override void PerformAutoLayout() {
        if (!_autoLayout || IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;
        var children = m_ChildComponents;
        var visibleCount = 0;

        foreach (var child in children)
            if (child.isVisibleSelf && child.enabled && child.gameObject.activeSelf && child.name != ExcludedControl)
                visibleCount++;

        if (visibleCount == 0) {
            IsPerformingAutoLayout = false;
            return;
        }

        var totalMain = 0f;
        var maxCross = 0f;

        foreach (var child in children) {
            if (!child.isVisibleSelf || !child.enabled || !child.gameObject.activeSelf || child.name == ExcludedControl)
                continue;

            if (_direction is FlexDirection.Column or FlexDirection.ColumnReverse) {
                totalMain += child.height + _rowGap;
                maxCross = Mathf.Max(maxCross, child.width);
            }
            else {
                totalMain += child.width + _columnGap;
                maxCross = Mathf.Max(maxCross, child.height);
            }
        }

        if (_direction is FlexDirection.Column or FlexDirection.ColumnReverse)
            totalMain -= _rowGap;
        else
            totalMain -= _columnGap;

        var contentWidth = _direction is FlexDirection.Column or FlexDirection.ColumnReverse ? maxCross : totalMain;
        var contentHeight = _direction is FlexDirection.Column or FlexDirection.ColumnReverse ? totalMain : maxCross;

        var mainPos = _direction is FlexDirection.ColumnReverse or FlexDirection.RowReverse ? totalMain : 0f;

        var currentIndex = 0;

        foreach (var child in children) {
            if (!child.isVisible || !child.enabled || !child.gameObject.activeSelf || child.name == ExcludedControl)
                continue;

            var x = _padding.Left;
            var y = _padding.Top;

            switch (_alignItems) {
                case FlexAlignItems.Center:
                    if (_direction is FlexDirection.Column or FlexDirection.ColumnReverse)
                        x = (width - child.width) / 2f;
                    else
                        y = (height - child.height) / 2f;
                    break;

                case FlexAlignItems.FlexEnd:
                    if (_direction is FlexDirection.Column or FlexDirection.ColumnReverse)
                        x = width - _padding.Right - child.width;
                    else
                        y = height - _padding.Bottom - child.height;
                    break;
            }

            var isLast = currentIndex == visibleCount - 1;

            switch (_direction) {
                case FlexDirection.Column:
                    child.relativePosition = new Vector3(x, _padding.Top + mainPos);
                    mainPos += child.height + (isLast ? 0f : _rowGap);
                    break;

                case FlexDirection.ColumnReverse:
                    child.relativePosition = new Vector3(x, _padding.Top + mainPos - child.height);
                    mainPos -= child.height + (isLast ? 0f : _rowGap);
                    break;

                case FlexDirection.Row:
                    child.relativePosition = new Vector3(_padding.Left + mainPos, y);
                    mainPos += child.width + (isLast ? 0f : _columnGap);
                    break;

                case FlexDirection.RowReverse:
                    child.relativePosition = new Vector3(_padding.Left + mainPos - child.width, y);
                    mainPos -= child.width + (isLast ? 0f : _columnGap);
                    break;
            }

            currentIndex++;
        }

        if (_autoFitChildrenHorizontally) {
            var newWidth = contentWidth + _padding.Horizontal;
            SetWidthIfChanged(newWidth);
        }

        if (_autoFitChildrenVertically) {
            var newHeight = contentHeight + _padding.Vertical;
            SetHeightIfChanged(newHeight);
        }

        IsPerformingAutoLayout = false;
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
    }
}