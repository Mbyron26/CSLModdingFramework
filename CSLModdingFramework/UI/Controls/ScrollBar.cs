using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class ScrollBar : UIInteractiveControl, IThemeable {
    protected SlicedSpriteView _thumbControl;
    protected ScrollOrientation _orientation;
    protected EdgeInsets _thumbPadding;
    protected bool _autoHide = true;
    protected bool _autoDisableButtons;
    protected UIComponent _incrementButton;
    protected UIComponent _decrementButton;
    protected float _buttonScrollStep = 10f;
    protected ThemeKind _theme;
    protected float _wheelScrollStep = 10f;
    private bool _dragging;
    private float _mouseOffset;
    private BackgroundRenderer _background;

    public override bool canFocus => (isEnabled && isVisible) || base.canFocus;
    public ScrollModel Model { get; private set; }
    public bool AllowArrowScroll { get; set; }
    public bool AllowMouseWheel { get; set; } = true;
    public bool ThumbVisible { get; private set; }
    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);

    public virtual ThemeKind Theme {
        get => _theme;
        set {
            if (_thumbControl == null) return;
            if (value == _theme) return;

            _theme = value;
            if (_theme == ThemeKind.ClassicLight) {
                _thumbControl.Colors.SetValues(UIColorPalette.ClassicLightPalette.SettingsSectionBackgroundHovered);
            }
            else {
                _thumbControl.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingsSectionColors);
            }
        }
    }

    public float WheelScrollStep {
        get => _wheelScrollStep;
        set {
            value = Mathf.Max(0f, value);
            if (!Mathf.Approximately(value, _wheelScrollStep)) {
                _wheelScrollStep = value;
            }
        }
    }

    public float ButtonScrollStep {
        get => _buttonScrollStep;
        set {
            value = Mathf.Max(0f, value);
            if (!Mathf.Approximately(value, _buttonScrollStep)) {
                _buttonScrollStep = value;
            }
        }
    }

    public UIComponent IncrementButton {
        get => _incrementButton;
        set {
            if (value == _incrementButton) return;

            if (_incrementButton != null) {
                _incrementButton.eventMouseDown -= IncrementPressed;
                _incrementButton.eventMouseHover -= IncrementPressed;
            }

            _incrementButton = value;

            if (_incrementButton != null) {
                _incrementButton.eventMouseDown += IncrementPressed;
                _incrementButton.eventMouseHover += IncrementPressed;
            }

            Invalidate();
        }
    }

    public UIComponent DecrementButton {
        get => _decrementButton;
        set {
            if (value == _decrementButton) return;

            if (_decrementButton != null) {
                _decrementButton.eventMouseDown -= DecrementPressed;
                _decrementButton.eventMouseHover -= DecrementPressed;
            }

            _decrementButton = value;

            if (_decrementButton != null) {
                _decrementButton.eventMouseDown += DecrementPressed;
                _decrementButton.eventMouseHover += DecrementPressed;
            }

            Invalidate();
        }
    }

    public bool AutoDisableButtons {
        get => _autoDisableButtons;
        set {
            if (value == _autoDisableButtons) return;
            _autoDisableButtons = value;
            Invalidate();
            UpdateAutoDisableButtons();
        }
    }

    public bool AutoHide {
        get => _autoHide;
        set {
            if (value == _autoHide) return;
            _autoHide = value;
            Invalidate();
            UpdateAutoHide();
        }
    }

    public EdgeInsets ThumbPadding {
        get => _thumbPadding;
        set {
            if (_orientation == ScrollOrientation.Horizontal) {
                value.Top = value.Bottom = 0;
            }
            else {
                value.Left = value.Right = 0;
            }

            if (value.EqualsApprox(_thumbPadding)) return;
            _thumbPadding = value;
            UpdateThumb();
        }
    }

    public ScrollOrientation Orientation {
        get => _orientation;
        set {
            if (value == _orientation) return;
            _orientation = value;
            Invalidate();
        }
    }

    public SlicedSpriteView ThumbControl {
        get => _thumbControl;
        set {
            if (value == _thumbControl) return;

            _thumbControl = value;
            Invalidate();
        }
    }

    public override void Update() {
        base.Update();
        Model?.OnUpdate(Time.deltaTime);
    }

    public override Vector2 CalculateMinimumSize() {
        var array = new Vector2[3];
        if (DecrementButton != null) {
            array[0] = DecrementButton.CalculateMinimumSize();
        }

        if (IncrementButton != null) {
            array[1] = IncrementButton.CalculateMinimumSize();
        }

        if (ThumbControl != null) {
            array[2] = ThumbControl.CalculateMinimumSize();
        }

        var zero = Vector2.zero;
        if (Orientation == ScrollOrientation.Horizontal) {
            zero.x = array[0].x + array[1].x + array[2].x;
            zero.y = Mathf.Max(new[] {
                array[0].y,
                array[1].y,
                array[2].y
            });
        }
        else {
            zero.x = Mathf.Max(new[] {
                array[0].x,
                array[1].x,
                array[2].x
            });
            zero.y = array[0].y + array[1].y + array[2].y;
        }

        return Vector2.Max(zero, base.CalculateMinimumSize());
    }

    protected override void OnKeyDown(UIKeyEventParameter p) {
        if (AllowArrowScroll && Model is not null) {
            if (Orientation == ScrollOrientation.Horizontal) {
                if (p.keycode == KeyCode.LeftArrow) {
                    Model.ScrollEase(-ButtonScrollStep);
                    p.Use();
                    return;
                }

                if (p.keycode == KeyCode.RightArrow) {
                    Model.ScrollEase(ButtonScrollStep);
                    p.Use();
                    return;
                }
            }
            else {
                if (p.keycode == KeyCode.UpArrow) {
                    Model.ScrollEase(-ButtonScrollStep);
                    p.Use();
                    return;
                }

                if (p.keycode == KeyCode.DownArrow) {
                    Model.ScrollEase(ButtonScrollStep);
                    p.Use();
                    return;
                }
            }
        }

        base.OnKeyDown(p);
    }

    protected override void OnMouseWheel(UIMouseEventParameter p) {
        if (!AllowMouseWheel || Model is null) return;

        Model.ScrollEase(_wheelScrollStep * -p.wheelDelta);
        p.Use();
    }

    protected override void OnMouseMove(UIMouseEventParameter p) {
        if (p.source == _incrementButton || p.source == _decrementButton) return;

        if ((p.source != this && p.source != _thumbControl) || !p.buttons.IsFlagSet(UIMouseButton.Left)) {
            base.OnMouseMove(p);
            return;
        }

        if (!_dragging || Model is null) return;

        var trackRect = this.GetScreenRect();
        if (ThumbControl != null && trackRect.HasValue) {
            var thumbRect = ThumbControl.GetScreenRect();
            if (thumbRect.HasValue) {
                var trackRectValue = trackRect.Value;
                var thumbRectValue = thumbRect.Value;

                var trackHeight = trackRectValue.height;
                var thumbHeight = thumbRectValue.height;
                var usableHeight = trackHeight - thumbHeight;

                var idealThumbYMin = Input.mousePosition.y + _mouseOffset - thumbHeight;

                var clampedYMin = Mathf.Clamp(
                    idealThumbYMin,
                    trackRectValue.yMin,
                    trackRectValue.yMax - thumbHeight
                );

                var ratio = 1f - (clampedYMin - trackRectValue.yMin) / usableHeight;
                ratio = Mathf.Clamp01(ratio);
                Model.ScrollToPercentage(ratio);
            }
        }

        p.Use();
    }

    protected override void OnMouseDown(UIMouseEventParameter p) {
        if (!p.buttons.IsFlagSet(UIMouseButton.Left)) {
            base.OnMouseDown(p);
            return;
        }

        Focus();

        if (p.source == ThumbControl) {
            _dragging = true;

            var mouse = Input.mousePosition;
            if (ThumbControl != null) {
                var thumbRect = ThumbControl.GetScreenRect();
                if (thumbRect.HasValue) {
                    _mouseOffset = thumbRect.Value.yMax - mouse.y;
                }
            }

            p.Use();
            return;
        }

        if (p.source == this && Model is not null) {
            var trackRect = this.GetScreenRect();
            if (!trackRect.HasValue) return;

            var track = trackRect.Value;
            var mouseY = Input.mousePosition.y;

            var localY = Mathf.Clamp(mouseY - track.yMin, 0f, track.height);

            var thumbHeight = ThumbControl?.height ?? 0f;
            var usableHeight = track.height - thumbHeight;

            var targetThumbYMin = localY - thumbHeight * 0.5f;

            targetThumbYMin = Mathf.Clamp(targetThumbYMin, 0f, usableHeight);

            var ratio = 1f - (targetThumbYMin / usableHeight);
            Model.ScrollEaseToPercentage(ratio);

            p.Use();
            return;
        }

        base.OnMouseDown(p);
    }

    protected override void OnMouseUp(UIMouseEventParameter p) {
        base.OnMouseUp(p);

        _dragging = false;
    }

    protected override void OnRebuildRenderData() {
        UpdateThumb();
        _background?.Render();
    }

    public void Bind(ScrollModel model) {
        if (Model != null) {
            Model.ScrollPositionChanged -= OnScrollChanged;
            Model.ContentSizeChanged -= OnContentOrViewportChanged;
            Model.ViewportSizeChanged -= OnContentOrViewportChanged;
        }

        Model = model;

        if (Model != null) {
            Model.ScrollPositionChanged += OnScrollChanged;
            Model.ContentSizeChanged += OnContentOrViewportChanged;
            Model.ViewportSizeChanged += OnContentOrViewportChanged;
        }

        UpdateThumb();
        UpdateAutoDisableButtons();
        UpdateAutoHide();
    }

    public void Unbind() {
        if (Model != null) {
            Model.ScrollPositionChanged -= OnScrollChanged;
            Model.ContentSizeChanged -= OnContentOrViewportChanged;
            Model.ViewportSizeChanged -= OnContentOrViewportChanged;
        }

        Model = null;
    }

    private void IncrementPressed(UIComponent sender, UIMouseEventParameter p) {
        if (!p.buttons.IsFlagSet(UIMouseButton.Left) || Model is null) return;

        Model.ScrollEase(ButtonScrollStep);
        p.Use();
    }

    private void DecrementPressed(UIComponent sender, UIMouseEventParameter p) {
        if (!p.buttons.IsFlagSet(UIMouseButton.Left) || Model is null) return;

        Model.ScrollEase(-ButtonScrollStep);
        p.Use();
    }

    private void UpdateAutoDisableButtons() {
        if (!_autoDisableButtons || !Application.isPlaying || Model == null) return;

        var pos = Model.ScrollPosition;
        var max = Model.MaxScrollOffset;
        if (_decrementButton != null)
            _decrementButton.isEnabled = pos > 0f;

        if (_incrementButton != null)
            _incrementButton.isEnabled = pos < max;
    }

    private void UpdateAutoHide() {
        if (!_autoHide || !Application.isPlaying || Model == null) return;

        if (Model.ContentSize <= Model.ViewportSize + 0.0001f) {
            Hide();
        }
        else {
            Show();
        }
    }

    private void UpdateThumb() {
        if (_thumbControl == null || !isVisible || Model == null) return;

        var contentSize = Model.ContentSize;
        var viewportSize = Model.ViewportSize;
        var scrollPos = Model.ScrollPosition;
        var maxScroll = Model.MaxScrollOffset;

        if (contentSize <= viewportSize) {
            ThumbVisible = _thumbControl.isVisible = false;
            return;
        }

        ThumbVisible = _thumbControl.isVisible = true;

        var trackLength = Orientation == ScrollOrientation.Horizontal
            ? width - ThumbPadding.Horizontal
            : height - ThumbPadding.Vertical;

        var percent = viewportSize / contentSize;
        var idealThumbLength = percent * trackLength;

        var thumbLength = Mathf.Max(
            idealThumbLength,
            Orientation == ScrollOrientation.Horizontal ? _thumbControl.minimumSize.x : _thumbControl.minimumSize.y
        );

        _thumbControl.size = Orientation == ScrollOrientation.Horizontal
            ? new Vector2(thumbLength, _thumbControl.height)
            : new Vector2(_thumbControl.width, thumbLength);

        var movableDistance = trackLength - thumbLength;
        var scrollPercent = scrollPos / maxScroll;

        var thumbOffset = scrollPercent * movableDistance;

        var startOffset = Orientation == ScrollOrientation.Horizontal
            ? new Vector3(ThumbPadding.Left, (height - _thumbControl.height) * 0.5f)
            : new Vector3((width - _thumbControl.width) * 0.5f, ThumbPadding.Top);

        var direction = Orientation == ScrollOrientation.Horizontal ? Vector3.right : Vector3.up;

        _thumbControl.relativePosition = startOffset + direction * thumbOffset;
    }

    private void OnScrollChanged(float pos) {
        UpdateThumb();
        UpdateAutoDisableButtons();
    }

    private void OnContentOrViewportChanged(float _) {
        UpdateThumb();
        UpdateAutoHide();
    }
}