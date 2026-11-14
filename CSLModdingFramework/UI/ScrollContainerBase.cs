using System;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Controls;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class ScrollContainerBase<TContent> : UIInteractiveControl where TContent : UIControl {
    protected TContent _contentContainer;
    protected ScrollSizeMode _sizeMode = ScrollSizeMode.Fixed;
    protected Size _minAutoFitSize;
    protected Size _maxAutoFitSize;
    protected ScrollOrientation _orientation = ScrollOrientation.Vertical;
    protected ScrollBar _horizontalScrollBar;
    protected ScrollBar _verticalScrollBar;
    protected ScrollBarPlacement _horizontalPlacement = ScrollBarPlacement.External;
    protected ScrollBarPlacement _verticalPlacement = ScrollBarPlacement.External;
    protected bool _autoReset;
    protected Vector2 _externalVerticalOffset = Vector2.zero;
    protected Vector2 _externalHorizontalOffset = Vector2.zero;
    protected EdgeInsets _scrollBarPositionPadding;
    protected EdgeInsets _clipPadding;
    private bool _isDragging;
    private Vector2 _lastPointerPos;
    private bool _resetNeeded;
    private bool _isInternalSizeChange;
    private BackgroundRenderer _background;
    private ForegroundRenderer _foreground;

    public event UIEventHandler<TContent, UIComponent> ItemAdded;

    public override bool canFocus => (isEnabled && isVisible) || base.canFocus;
    public ScrollModel HorizontalModel { get; private set; }
    public ScrollModel VerticalModel { get; private set; }
    public bool AllowDragScroll { get; set; }
    public bool AllowMouseWheel { get; set; } = true;
    public bool AllowArrowScroll { get; set; }
    public float MouseWheelSpeed { get; set; } = 50f;
    private float ArrowScrollSpeed { get; set; } = 10f;
    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);
    public ForegroundRenderer Foreground => _foreground ??= new ForegroundRenderer(this);

    public EdgeInsets ScrollBarPositionPadding {
        get => _scrollBarPositionPadding;
        set {
            if (value == _scrollBarPositionPadding) return;

            _scrollBarPositionPadding = value;
            UpdateScrollBarPosition();
            Invalidate();
        }
    }

    public Vector2 ExternalVerticalOffset {
        get => _externalVerticalOffset;
        set {
            if (value == _externalVerticalOffset) return;

            _externalVerticalOffset = value;
            UpdateScrollBarPosition();
            Invalidate();
        }
    }

    public Vector2 ExternalHorizontalOffset {
        get => _externalHorizontalOffset;
        set {
            if (value == _externalHorizontalOffset) return;

            _externalHorizontalOffset = value;
            UpdateScrollBarPosition();
            Invalidate();
        }
    }

    public bool AutoReset {
        get => _autoReset;
        set {
            if (value == _autoReset) return;
            _autoReset = value;
            if (value)
                Reset();
        }
    }

    public ScrollBarPlacement VerticalPlacement {
        get => _verticalPlacement;
        set {
            if (value == _verticalPlacement) return;
            _verticalPlacement = value;
            UpdateScrollBarPosition();
            Invalidate();
        }
    }

    public ScrollBarPlacement HorizontalPlacement {
        get => _horizontalPlacement;
        set {
            if (value == _horizontalPlacement) return;
            _horizontalPlacement = value;
            UpdateScrollBarPosition();
            Invalidate();
        }
    }

    public ScrollBar HorizontalScrollBar {
        get => _horizontalScrollBar;
        set {
            _horizontalScrollBar = value;
            UpdateScrollBarPosition();
        }
    }

    public ScrollBar VerticalScrollBar {
        get => _verticalScrollBar;
        set {
            _verticalScrollBar = value;
            UpdateScrollBarPosition();
        }
    }

    public ScrollOrientation ScrollOrientation {
        get => _orientation;
        set {
            if (value == _orientation) return;
            _orientation = value;
            UpdateContentConstraints();
            Invalidate();
        }
    }

    public EdgeInsets ClipPadding {
        get => _clipPadding;
        set {
            if (value.EqualsApprox(_clipPadding)) return;

            _clipPadding = value;
            Invalidate();
        }
    }

    public Size AutoFitMaxSize {
        get => _maxAutoFitSize;
        set {
            if (value.Approximately(_maxAutoFitSize)) return;
            _maxAutoFitSize = value;
            UpdateSize();
            Invalidate();
        }
    }

    public Size AutoFitMinSize {
        get => _minAutoFitSize;
        set {
            if (value.Approximately(_minAutoFitSize)) return;
            _minAutoFitSize = value;
            UpdateSize();
            Invalidate();
        }
    }

    public ScrollSizeMode SizeMode {
        get => _sizeMode;
        set {
            if (value == _sizeMode) return;
            _sizeMode = value;
            UpdateSize();
            Invalidate();
        }
    }

    public TContent ContentContainer => _contentContainer;

    protected abstract void CreateContentContainer();

    public virtual T AddItem<T>() where T : UIComponent {
        var item = _contentContainer.AddUIComponent<T>();
        OnItemAdded(item);
        return item;
    }

    public virtual UIComponent AddItem(Type type) {
        var item = _contentContainer.AddUIComponent(type);
        OnItemAdded(item);
        return item;
    }

    protected virtual void OnContentContainerSizeChanged(UIComponent component, Vector2 value) {
        if (_isInternalSizeChange) return;

        _isInternalSizeChange = true;
        UpdateSize();
        VerticalModel?.SetContentSize(value.y);
        HorizontalModel?.SetContentSize(value.x);
        UpdateScrollBarSize();
        UpdateScrollBarPosition();
        Invalidate();
        _isInternalSizeChange = false;
    }

    protected virtual Vector3[] GetClippingCorners() {
        var num = PixelsToUnits();
        var matrix = cachedTransform.localToWorldMatrix;
        var newSize = GetClippingSize();
        var upperLeft = pivot.TransformToUpperLeft(newSize, arbitraryPivotOffset);
        var upperRight = upperLeft + new Vector3(newSize.x, 0f);
        var bottomLeft = upperLeft + new Vector3(0f, -newSize.y);
        var bottomRight = upperRight + new Vector3(0f, -newSize.y);

        m_CachedCorners[0] = matrix.MultiplyPoint(upperLeft * num);
        m_CachedCorners[1] = matrix.MultiplyPoint(upperRight * num);
        m_CachedCorners[2] = matrix.MultiplyPoint(bottomRight * num);
        m_CachedCorners[3] = matrix.MultiplyPoint(bottomLeft * num);
        return m_CachedCorners;
    }

    protected virtual Vector2 GetClippingSize() {
        var newWidth = size.x;
        var newHeight = size.y;

        if (_verticalScrollBar != null && _verticalScrollBar.isVisible) {
            if (_verticalPlacement == ScrollBarPlacement.External)
                newWidth += _verticalScrollBar.width + ExternalVerticalOffset.x;
        }

        if (_horizontalScrollBar != null && _horizontalScrollBar.isVisible) {
            if (_horizontalPlacement == ScrollBarPlacement.External) {
                newHeight += _horizontalScrollBar.height + ExternalHorizontalOffset.y;
            }
        }

        return new Vector2(newWidth, newHeight);
    }

    protected virtual void OnItemAdded(UIComponent item) => ItemAdded?.Invoke(_contentContainer, item);

    protected virtual void UpdateContentConstraints() {
        if (_contentContainer == null) return;

        var fitWidth = _orientation != ScrollOrientation.Vertical;
        var fitHeight = _orientation != ScrollOrientation.Horizontal;

        if (!fitWidth)
            _contentContainer.width = width;

        if (!fitHeight)
            _contentContainer.height = height;

        if (_orientation == ScrollOrientation.None)
            _contentContainer.size = size;
    }

    protected virtual void UpdateContentContainerSize(Vector2 newSize) {
        if (_contentContainer == null) return;

        VerticalModel?.SetContentSize(newSize.y);
        HorizontalModel?.SetContentSize(newSize.x);
    }

    protected virtual Vector2 GetContentSize() => _contentContainer == null ? Vector2.zero : _contentContainer.size;

    protected virtual void UpdateScrollBarSize() {
        if (_verticalScrollBar != null) {
            _verticalScrollBar.height = height - _scrollBarPositionPadding.Vertical;
        }

        if (_horizontalScrollBar != null) {
            _horizontalScrollBar.width = width - _scrollBarPositionPadding.Horizontal;
        }
    }

    protected virtual void UpdateScrollBarPosition() {
        if (_verticalScrollBar != null) {
            switch (_verticalPlacement) {
                case ScrollBarPlacement.External:
                    _verticalScrollBar.relativePosition = new Vector2(size.x, _scrollBarPositionPadding.Top) + ExternalVerticalOffset;
                    break;
                case ScrollBarPlacement.Inset:
                case ScrollBarPlacement.Overlay:
                default:
                    _verticalScrollBar.relativePosition = new Vector2(size.x - _verticalScrollBar.width, _scrollBarPositionPadding.Top);
                    break;
            }
        }

        if (_horizontalScrollBar != null) {
            switch (_horizontalPlacement) {
                case ScrollBarPlacement.External:
                    _horizontalScrollBar.relativePosition = new Vector2(_scrollBarPositionPadding.Left, size.y) + ExternalHorizontalOffset;
                    break;
                case ScrollBarPlacement.Inset:
                case ScrollBarPlacement.Overlay:
                default:
                    _horizontalScrollBar.relativePosition = new Vector2(_scrollBarPositionPadding.Left, size.y - _horizontalScrollBar.height);
                    break;
            }
        }
    }

    public override void Awake() {
        base.Awake();
        size = new Vector2(100, 50);
        m_ClipChildren = true;

        CreateContentContainer();
    }

    public override void Update() {
        base.Update();
        var dt = Time.deltaTime;
        HorizontalModel?.OnUpdate(dt);
        VerticalModel?.OnUpdate(dt);
    }

    public override void LateUpdate() {
        base.LateUpdate();

        if (!_autoReset || !_resetNeeded) return;

        _resetNeeded = false;
        HorizontalModel?.ScrollToStart();
        VerticalModel?.ScrollToStart();
    }

    protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution) {
        base.OnResolutionChanged(previousResolution, currentResolution);

        _resetNeeded = true;
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        if (_isInternalSizeChange) {
            VerticalModel?.SetViewportSize(height);
            HorizontalModel?.SetViewportSize(width);
            return;
        }

        _isInternalSizeChange = true;
        UpdateContentConstraints();
        UpdateScrollBarSize();
        UpdateScrollBarPosition();

        VerticalModel?.SetViewportSize(height);
        if (_contentContainer != null)
            VerticalModel?.SetContentSize(_contentContainer.height);

        HorizontalModel?.SetViewportSize(width);
        if (_contentContainer != null)
            HorizontalModel?.SetContentSize(_contentContainer.width);

        if (_autoReset)
            Reset();

        _isInternalSizeChange = false;
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
    }

    protected override Plane[] GetClippingPlanes() {
        if (!clipChildren) return null;

        var corners = GetClippingCorners();
        var vector = transform.TransformDirection(Vector3.right);
        var vector2 = transform.TransformDirection(Vector3.left);
        var vector3 = transform.TransformDirection(Vector3.up);
        var vector4 = transform.TransformDirection(Vector3.down);

        m_CachedClippingPlanes[0] = new Plane(vector, corners[0]);
        m_CachedClippingPlanes[1] = new Plane(vector2, corners[1]);
        m_CachedClippingPlanes[2] = new Plane(vector3, corners[2]);
        m_CachedClippingPlanes[3] = new Plane(vector4, corners[0]);
        return m_CachedClippingPlanes;
    }

    protected override void OnMouseWheel(UIMouseEventParameter p) {
        if (AllowMouseWheel && !p.used) {
            var delta = -p.wheelDelta * MouseWheelSpeed;

            switch (ScrollOrientation) {
                case ScrollOrientation.Vertical:
                    VerticalModel?.ScrollImmediate(delta);
                    break;

                case ScrollOrientation.Horizontal:
                    HorizontalModel?.ScrollImmediate(delta);
                    break;

                case ScrollOrientation.Bidirectional:
                    if (Input.GetKey(KeyCode.LeftShift))
                        HorizontalModel?.ScrollImmediate(delta);
                    else
                        VerticalModel?.ScrollImmediate(delta);
                    break;
                case ScrollOrientation.None:
                default:
                    break;
            }

            p.Use();
        }

        base.OnMouseWheel(p);
    }

    protected override void OnKeyDown(UIKeyEventParameter p) {
        if (!AllowArrowScroll || p.used) {
            base.OnKeyDown(p);
            return;
        }

        var horizontalStep = _horizontalScrollBar != null
            ? _horizontalScrollBar.ButtonScrollStep
            : ArrowScrollSpeed;

        var verticalStep = _verticalScrollBar != null
            ? _verticalScrollBar.ButtonScrollStep
            : ArrowScrollSpeed;

        switch (p.keycode) {
            case KeyCode.LeftArrow:
                HorizontalModel?.ScrollImmediate(-horizontalStep);
                p.Use();
                break;

            case KeyCode.RightArrow:
                HorizontalModel?.ScrollImmediate(horizontalStep);
                p.Use();
                break;

            case KeyCode.UpArrow:
                VerticalModel?.ScrollImmediate(-verticalStep);
                p.Use();
                break;

            case KeyCode.DownArrow:
                VerticalModel?.ScrollImmediate(verticalStep);
                p.Use();
                break;
        }

        base.OnKeyDown(p);
    }

    protected override void OnVisibilityChanged() {
        base.OnVisibilityChanged();
        if (!isVisible || !_autoReset) return;
        Reset();
    }

    protected override void OnMouseDown(UIMouseEventParameter p) {
        base.OnMouseDown(p);

        if (!AllowDragScroll || p.used) return;

        _isDragging = true;
        _lastPointerPos = p.position;
        p.Use();
    }

    protected override void OnMouseMove(UIMouseEventParameter p) {
        base.OnMouseMove(p);

        if (!AllowDragScroll || !_isDragging) return;

        var delta = p.position - _lastPointerPos;
        _lastPointerPos = p.position;

        var scrollDelta = -delta;

        switch (ScrollOrientation) {
            case ScrollOrientation.Vertical:
                VerticalModel?.SetScrollPosition(VerticalModel.ScrollPosition + scrollDelta.y);
                break;

            case ScrollOrientation.Horizontal:
                HorizontalModel?.SetScrollPosition(HorizontalModel.ScrollPosition + scrollDelta.x);
                break;

            case ScrollOrientation.Bidirectional:
                HorizontalModel?.SetScrollPosition(HorizontalModel.ScrollPosition + scrollDelta.x);
                VerticalModel?.SetScrollPosition(VerticalModel.ScrollPosition + scrollDelta.y);
                break;

            case ScrollOrientation.None:
                return;
        }

        p.Use();
    }

    protected override void OnMouseUp(UIMouseEventParameter p) {
        base.OnMouseUp(p);
        _isDragging = false;
    }

    protected override void OnDragEnd(UIDragEventParameter p) {
        base.OnDragEnd(p);
        _isDragging = false;
    }

    public ScrollModel EnsureHorizontalModel() {
        if (HorizontalModel == null)
            BindHorizontal(new ScrollModel());
        return HorizontalModel;
    }

    public ScrollModel EnsureVerticalModel() {
        if (VerticalModel == null)
            BindVertical(new ScrollModel());
        return VerticalModel;
    }

    public void BindHorizontal(ScrollModel model) {
        if (HorizontalModel != null)
            UnbindHorizontal(HorizontalModel);

        HorizontalModel = model;

        if (HorizontalModel != null) {
            BindHorizontalInternal(HorizontalModel);
            HorizontalModel.SetViewportSize(size.x);
            HorizontalModel.SetContentSize(GetContentSize().x);
            HorizontalModel.SetScrollPosition(HorizontalModel.ScrollPosition);
        }
    }

    public void BindVertical(ScrollModel model) {
        if (VerticalModel != null)
            UnbindVertical(VerticalModel);

        VerticalModel = model;

        if (VerticalModel != null) {
            BindVerticalInternal(VerticalModel);
            VerticalModel.SetViewportSize(size.y);
            VerticalModel.SetContentSize(GetContentSize().y);
            VerticalModel.SetScrollPosition(VerticalModel.ScrollPosition);
        }
    }

    public ScrollBar AddVerticalScrollBar(float scrollbarWidth, ThemeKind style) => _verticalScrollBar = AddScrollBar(true, scrollbarWidth, style);

    public ScrollBar AddHorizontalScrollBar(float scrollbarHeight, ThemeKind style) => _horizontalScrollBar = AddScrollBar(false, scrollbarHeight, style);

    public ScrollBar AddScrollBar(bool isVertical, float thickness, ThemeKind style) {
        var scrollbar = AddUIComponent<ScrollBar>();
        scrollbar.size = isVertical ? new Vector2(thickness, size.y) : new Vector2(size.x, thickness);
        scrollbar.WheelScrollStep = 50f;

        var thumb = scrollbar.AddUIComponent<SlicedSpriteView>();
        thumb.relativePosition = Vector2.zero;
        if (isVertical)
            thumb.width = scrollbar.width;
        else
            thumb.height = scrollbar.height;
        thumb.Atlas = DefaultAtlas.Instance;
        thumb.SpriteName = DefaultAtlas.SmoothRectangleRadius4;
        thumb.color = UIColorPalette.White;
        scrollbar.ThumbControl = thumb;
        scrollbar.Theme = style;
        UpdateScrollBarPosition();
        return scrollbar;
    }

    public void ScrollToLeft() => HorizontalModel?.ScrollToStart();
    public void ScrollToRight() => HorizontalModel?.ScrollToEnd();
    public void ScrollToTop() => VerticalModel?.ScrollToStart();
    public void ScrollToBottom() => VerticalModel?.ScrollToEnd();
    public void Reset() => _resetNeeded = true;

    protected void UpdateSize() {
        if (_sizeMode != ScrollSizeMode.AutoFit) return;

        var contentSize = GetContentSize();

        var newWidth = Mathf.Clamp(contentSize.x, _minAutoFitSize.Width, _maxAutoFitSize.Width);
        var newHeight = Mathf.Clamp(contentSize.y, _minAutoFitSize.Height, _maxAutoFitSize.Height);
        if (Mathf.Approximately(size.x, newWidth) &&
            Mathf.Approximately(size.y, newHeight))
            return;
        size = new Vector2(newWidth, newHeight);
    }

    private void UpdateContentPosition() {
        if (_contentContainer == null) return;

        var x = -(HorizontalModel?.ScrollPosition ?? 0f);
        var y = -(VerticalModel?.ScrollPosition ?? 0f);

        _contentContainer.relativePosition = new Vector2(x, y);
    }

    private void BindHorizontalInternal(ScrollModel model) {
        model.ScrollPositionChanged += OnHorizontalScrollPositionChanged;
    }

    private void BindVerticalInternal(ScrollModel model) {
        model.ScrollPositionChanged += OnVerticalScrollPositionChanged;
    }

    private void UnbindHorizontal(ScrollModel model) {
        model.ScrollPositionChanged -= OnHorizontalScrollPositionChanged;
    }

    private void UnbindVertical(ScrollModel model) {
        model.ScrollPositionChanged -= OnVerticalScrollPositionChanged;
    }

    private void OnHorizontalScrollPositionChanged(float _) => UpdateContentPosition();

    private void OnVerticalScrollPositionChanged(float _) => UpdateContentPosition();
}