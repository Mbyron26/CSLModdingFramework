using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class ScrollViewer : UIInteractiveControl, IAutoLayout {
    protected UIComponent _content;
    protected ScrollSizePolicy _sizeMode = ScrollSizePolicy.Fixed;
    protected Size _minFitSize;
    protected Size _maxFitSize = new(4096, 4096);
    protected ScrollOrientation _orientation;
    protected ScrollBar _horizontalScrollBar;
    protected ScrollBar _verticalScrollBar;
    protected EdgeInsets _verticalScrollBarMargin;
    protected EdgeInsets _horizontalScrollBarMargin;
    protected Vector3 _verticalScrollBarPositionOffset;
    protected Vector3 _horizontalScrollBarPositionOffset;
    protected ScrollModel _horizontalScrollModel;
    protected ScrollModel _verticalScrollModel;
    protected int _autoLayoutSuspendCount;
    protected EdgeInsets _padding;
    protected BackgroundRenderer _background;
    protected ForegroundRenderer _foreground;
    private bool _isDragging;
    private Vector2 _lastPointerPos;

    public virtual ScrollOrientation Orientation {
        get => _orientation;
        set => SetOrientation(value);
    }

    public ScrollBar HorizontalScrollBar => _horizontalScrollBar;
    public ScrollBar VerticalScrollBar => _verticalScrollBar;
    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);
    public ForegroundRenderer Foreground => _foreground ??= new ForegroundRenderer(this);

    public virtual EdgeInsets Padding {
        get => _padding;
        set {
            if (_padding.EqualsApprox(value)) return;

            _padding = value;
            UpdateSize();
        }
    }

    public virtual Vector3 HorizontalScrollBarPositionOffset {
        get => _horizontalScrollBarPositionOffset;
        set {
            if (_horizontalScrollBarPositionOffset == value) return;

            _horizontalScrollBarPositionOffset = value;
            UpdateScrollBarPosition();
        }
    }

    public virtual Vector3 VerticalScrollBarPositionOffset {
        get => _verticalScrollBarPositionOffset;
        set {
            if (_verticalScrollBarPositionOffset == value) return;

            _verticalScrollBarPositionOffset = value;
            UpdateScrollBarPosition();
        }
    }

    public virtual EdgeInsets HorizontalScrollBarMargin {
        get => _horizontalScrollBarMargin;
        set {
            if (_horizontalScrollBarMargin.EqualsApprox(value)) return;

            _horizontalScrollBarMargin = value;
            UpdateScrollBarSize();
            UpdateScrollBarPosition();
        }
    }

    public virtual EdgeInsets VerticalScrollBarMargin {
        get => _verticalScrollBarMargin;
        set {
            if (_verticalScrollBarMargin.EqualsApprox(value)) return;

            _verticalScrollBarMargin = value;
            UpdateScrollBarSize();
            UpdateScrollBarPosition();
        }
    }

    public virtual Size MaxFitSize {
        get => _maxFitSize;
        set {
            if (_maxFitSize.Approximately(value)) return;

            _maxFitSize = value;
            UpdateSize();
        }
    }

    public virtual Size MinFitSize {
        get => _minFitSize;
        set {
            if (_minFitSize.Approximately(value)) return;

            _minFitSize = value;
            UpdateSize();
        }
    }

    public virtual ScrollSizePolicy SizeMode {
        get => _sizeMode;
        set {
            if (_sizeMode == value) return;

            _sizeMode = value;
            UpdateSize();
        }
    }

    public virtual UIComponent Content {
        get => _content;
        set => SetContent(value);
    }

    public override bool canFocus => (isEnabled && isVisible) || m_CanFocus;

    public float? VerticalScrollBarHeightOverride { get; set; }
    public float? HorizontalScrollBarWidthOverride { get; set; }
    public Vector2 VerticalScrollBarOffset { get; set; }
    public Vector2 HorizontalScrollBarOffset { get; set; }
    public ScrollResetMode ContentChangeResetMode { get; set; }
    public bool AllowDragScroll { get; set; }
    public bool AllowMouseWheel { get; set; } = true;
    public bool AllowArrowScroll { get; set; }
    public float MouseWheelSpeed { get; set; } = 50f;
    private float ArrowScrollSpeed { get; set; } = 10f;
    public bool IsPerformingAutoLayout { get; protected set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        _verticalScrollModel?.SetViewportSize(height - _padding.Vertical);
        _horizontalScrollModel?.SetViewportSize(width - _padding.Horizontal);
        UpdateScrollBarSize();
        UpdateScrollBarPosition();

        if (_content != null) {
            _verticalScrollModel?.SetContentSize(_content.height);
            _horizontalScrollModel?.SetContentSize(_content.width);
        }

        IsPerformingAutoLayout = false;
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    protected virtual void SetOrientation(ScrollOrientation orientation) {
        if (_orientation == orientation) return;

        _orientation = orientation;

        switch (orientation) {
            case ScrollOrientation.None:
                DisableHorizontal();
                DisableVertical();
                break;

            case ScrollOrientation.Vertical:
                DisableHorizontal();
                EnableVertical();
                break;

            case ScrollOrientation.Horizontal:
                DisableVertical();
                EnableHorizontal();
                break;

            case ScrollOrientation.Bidirectional:
                EnableHorizontal();
                EnableVertical();
                break;
        }

        InvalidateAutoLayout();
    }

    public T SetContent<T>() where T : UIComponent {
        var content = AddUIComponent<T>();
        Content = content;
        return content;
    }

    protected virtual void SetContent(UIComponent value) {
        if (value == null || _content == value) return;

        if (_content != null) {
            _content.eventSizeChanged -= OnContentSizeChanged;
            _content.DestroySelf();
        }

        _content = value;
        if (_content.size == Vector2.zero) {
            _content.size = size - new Vector2(_padding.Horizontal, _padding.Vertical);
        }

        _verticalScrollModel?.SetContentSize(_content.height);
        _horizontalScrollModel?.SetContentSize(_content.width);
        UpdateContentPosition();
        _content.eventSizeChanged += OnContentSizeChanged;
        InvalidateAutoLayout();
    }

    protected virtual void OnContentSizeChanged(UIComponent component, Vector2 value) {
        UpdateSize();
        _verticalScrollModel?.SetContentSize(value.y);
        _horizontalScrollModel?.SetContentSize(value.x);
        UpdateContentPosition();
    }

    protected virtual void UpdateScrollBarSize() {
        if (_verticalScrollBar != null) {
            _verticalScrollBar.SetHeightIfChanged(height - _verticalScrollBarMargin.Vertical);
        }

        if (_horizontalScrollBar != null) {
            _horizontalScrollBar.SetWidthIfChanged(width - _verticalScrollBarMargin.Horizontal);
        }
    }

    protected virtual void UpdateScrollBarPosition() {
        if (_verticalScrollBar != null) {
            _verticalScrollBar.relativePosition = new Vector3(width - _verticalScrollBar.width, _verticalScrollBarMargin.Top) + _verticalScrollBarPositionOffset;
        }

        if (_horizontalScrollBar != null) {
            _horizontalScrollBar.relativePosition = new Vector3(_horizontalScrollBarMargin.Left, height - _horizontalScrollBar.height) + _horizontalScrollBarPositionOffset;
        }
    }

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(100, 50);
        m_ClipChildren = true;
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended) {
            PerformAutoLayout();
        }

        base.OnSizeChanged();
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
    }

    protected override Plane[] GetClippingPlanes() => clipChildren ? GetPaddedClippingPlanes(_padding) : null;

    protected override void OnMouseWheel(UIMouseEventParameter p) {
        if (AllowMouseWheel && !p.used) {
            var delta = -p.wheelDelta * MouseWheelSpeed;

            switch (_orientation) {
                case ScrollOrientation.Vertical:
                    _verticalScrollModel?.ScrollImmediate(delta);
                    break;

                case ScrollOrientation.Horizontal:
                    _horizontalScrollModel?.ScrollImmediate(delta);
                    break;

                case ScrollOrientation.Bidirectional:
                    if (Input.GetKey(KeyCode.LeftShift))
                        _horizontalScrollModel?.ScrollImmediate(delta);
                    else
                        _verticalScrollModel?.ScrollImmediate(delta);
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
                _horizontalScrollModel?.ScrollImmediate(-horizontalStep);
                p.Use();
                break;

            case KeyCode.RightArrow:
                _horizontalScrollModel?.ScrollImmediate(horizontalStep);
                p.Use();
                break;

            case KeyCode.UpArrow:
                _verticalScrollModel?.ScrollImmediate(-verticalStep);
                p.Use();
                break;

            case KeyCode.DownArrow:
                _verticalScrollModel?.ScrollImmediate(verticalStep);
                p.Use();
                break;
        }

        base.OnKeyDown(p);
    }

    // protected override void OnVisibilityChanged() {
    //     base.OnVisibilityChanged();
    //     
    //     if (!isVisible || !_autoReset) return;
    //     Reset();
    // }

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

        switch (_orientation) {
            case ScrollOrientation.Vertical:
                _verticalScrollModel?.SetScrollPosition(_verticalScrollModel.ScrollPosition + scrollDelta.y);
                break;

            case ScrollOrientation.Horizontal:
                _horizontalScrollModel?.SetScrollPosition(_horizontalScrollModel.ScrollPosition + scrollDelta.x);
                break;

            case ScrollOrientation.Bidirectional:
                _horizontalScrollModel?.SetScrollPosition(_horizontalScrollModel.ScrollPosition + scrollDelta.x);
                _verticalScrollModel?.SetScrollPosition(_verticalScrollModel.ScrollPosition + scrollDelta.y);
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

    public void ScrollToLeft() => _horizontalScrollModel?.ScrollToStart();
    public void ScrollToRight() => _horizontalScrollModel?.ScrollToEnd();
    public void ScrollToTop() => _verticalScrollModel?.ScrollToStart();
    public void ScrollToBottom() => _verticalScrollModel?.ScrollToEnd();

    protected void UpdateSize() {
        if (_sizeMode == ScrollSizePolicy.Fixed) return;

        var contentSize = _content == null ? Vector2.zero : _content.size;

        var targetWidth = width;
        var targetHeight = height;

        var paddedWidth = contentSize.x + _padding.Horizontal;
        var paddedHeight = contentSize.y + _padding.Vertical;

        switch (_sizeMode) {
            case ScrollSizePolicy.FitWidth:
                targetWidth = Mathf.Clamp(paddedWidth, _minFitSize.Width, _maxFitSize.Width);
                break;

            case ScrollSizePolicy.FitHeight:
                targetHeight = Mathf.Clamp(paddedHeight, _minFitSize.Height, _maxFitSize.Height);
                break;

            case ScrollSizePolicy.FitContent:
                targetWidth = Mathf.Clamp(paddedWidth, _minFitSize.Width, _maxFitSize.Width);
                targetHeight = Mathf.Clamp(paddedHeight, _minFitSize.Height, _maxFitSize.Height);
                break;
        }

        SetSizeIfChanged(new Vector2(targetWidth, targetHeight));
    }

    private void EnableVertical() {
        if (_verticalScrollModel == null) {
            _verticalScrollModel = new ScrollModel();
            BindScrollModel(_verticalScrollModel);
            _verticalScrollModel.SetViewportSize(size.y - _padding.Vertical);

            if (_content != null)
                _verticalScrollModel.SetContentSize(_content.height);

            _verticalScrollModel.SetScrollPosition(_verticalScrollModel.ScrollPosition);
        }

        if (_verticalScrollBar == null) {
            _verticalScrollBar = AddScrollBar(true);
            _verticalScrollBar.Bind(_verticalScrollModel);
            UpdateScrollBarSize();
            UpdateScrollBarPosition();
        }
    }

    private void EnableHorizontal() {
        if (_horizontalScrollModel == null) {
            _horizontalScrollModel = new ScrollModel();
            BindScrollModel(_horizontalScrollModel);
            _horizontalScrollModel.SetViewportSize(size.x);

            if (_content != null)
                _horizontalScrollModel.SetContentSize(_content.width);

            _horizontalScrollModel.SetScrollPosition(_horizontalScrollModel.ScrollPosition);
        }

        if (_horizontalScrollBar == null) {
            _horizontalScrollBar = AddScrollBar(false);
            _horizontalScrollBar.Bind(_horizontalScrollModel);
            UpdateScrollBarSize();
            UpdateScrollBarPosition();
        }
    }

    private void DisableVertical() {
        if (_verticalScrollBar != null) {
            _verticalScrollBar.Unbind();
            _verticalScrollBar.DestroySelf();
            _verticalScrollBar = null;
        }

        if (_verticalScrollModel != null) {
            UnbindScrollModel(_verticalScrollModel);
            _verticalScrollModel = null;
        }
    }

    private void DisableHorizontal() {
        if (_horizontalScrollBar != null) {
            _horizontalScrollBar.Unbind();
            _horizontalScrollBar.DestroySelf();
            _horizontalScrollBar = null;
        }

        if (_horizontalScrollModel != null) {
            UnbindScrollModel(_horizontalScrollModel);
            _horizontalScrollModel = null;
        }
    }

    private ScrollBar AddScrollBar(bool isVertical) {
        var scrollbar = AddUIComponent<ScrollBar>();
        scrollbar.size = isVertical ? new Vector2(8, size.y) : new Vector2(size.x, 8);
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
        scrollbar.Theme=ThemeKind.ClassicDark;
        UpdateScrollBarPosition();
        return scrollbar;
    }

    private void BindScrollModel(ScrollModel model) => model.ScrollPositionChanged += OnScrollPositionChanged;

    private void UnbindScrollModel(ScrollModel model) => model.ScrollPositionChanged -= OnScrollPositionChanged;

    private void OnScrollPositionChanged(float _) => UpdateContentPosition();

    private void UpdateContentPosition() {
        if (_content == null) return;

        var x = _padding.Left - (_horizontalScrollModel?.ScrollPosition ?? 0f);
        var y = _padding.Top - (_verticalScrollModel?.ScrollPosition ?? 0f);

        _content.relativePosition = new Vector2(x, y);
    }

    public enum ScrollResetMode {
        None,
        ToStart,
        ToEnd
    }

    public enum ScrollSizePolicy {
        Fixed,
        FitWidth,
        FitHeight,
        FitContent
    }
}