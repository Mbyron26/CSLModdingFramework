using System;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class SliderBase : UIInteractiveControl, IAutoLayout, IThemeable {
    protected int _autoLayoutSuspendCount;
    protected Orientation _direction;
    protected IAtlas _fillAtlas;
    protected UIRenderData _fillRenderData;
    protected float _maxValue = 10f;
    protected float _minValue;
    protected float _normalizedValue;
    protected EdgeInsets _padding;
    protected float _stepSize;
    protected ThemeKind _theme;
    protected IAtlas _thumbAtlas;
    protected UIRenderData _thumbRenderData;
    protected float _thumbScale = 1f;
    protected Vector2 _thumbSize = new(20, 20);
    protected IAtlas _trackAtlas;
    protected UIRenderData _trackRenderData;
    protected Vector3 _trackRenderPosition;
    protected float _value;

    public event Action<float> ValueChanged;

    public virtual Orientation Direction {
        get => _direction;
        set => _direction = value;
    }

    public virtual float MaxValue {
        get => _maxValue;
        set {
            _maxValue = value;
            if (_maxValue < _minValue)
                _minValue = _maxValue;

            SetValue(_value);
        }
    }

    public virtual float MinValue {
        get => _minValue;
        set {
            _minValue = value;
            if (_maxValue < _minValue)
                _maxValue = _minValue;

            SetValue(_value);
        }
    }

    public virtual float NormalizedValue {
        get => _normalizedValue;
        set {
            var nv = Mathf.Clamp(value, 0f, 1f);
            var v = _minValue + nv * (_maxValue - _minValue);
            SetValue(v);
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

    public virtual float StepSize {
        get => _stepSize;
        set {
            _stepSize = Mathf.Max(0f, value);
            SetValue(_value);
        }
    }

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public virtual Vector2 ThumbSize {
        get => _thumbSize;
        set {
            if (value.Equals(_thumbSize)) return;

            _thumbSize = value;
            Invalidate();
        }
    }

    public virtual float Value {
        get => _value;
        set => SetValue(value);
    }

    public override bool canFocus => (isEnabled && isVisible) || canFocus;
    public float ArrowCtrlMultiplier { get; set; } = 0.1f;
    public float ArrowKeyStep { get; set; } = 1f;
    public float ArrowShiftMultiplier { get; set; } = 10f;
    public bool EnableArrowKeys { get; set; }
    public bool EnableMouseWheel { get; set; }

    public IAtlas FillAtlas {
        get => _fillAtlas;
        set {
            if (Equals(value, _fillAtlas)) return;

            _fillAtlas = value;
            Invalidate();
        }
    }

    public ColorStateValue FillColors { get; private set; }
    public SpriteStateValue FillSprites { get; private set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;
    public bool IsPerformingAutoLayout { get; protected set; }

    public IAtlas ThumbAtlas {
        get => _thumbAtlas;
        set {
            if (Equals(value, _thumbAtlas)) return;

            _thumbAtlas = value;
            Invalidate();
        }
    }

    public ColorStateValue ThumbColors { get; private set; }

    public float ThumbScale {
        get => _thumbScale;
        set {
            if (Mathf.Approximately(_thumbScale, value)) return;

            _thumbScale = value;
            Invalidate();
        }
    }

    public SpriteStateValue ThumbSprites { get; private set; }

    public IAtlas TrackAtlas {
        get => _trackAtlas;
        set {
            if (Equals(value, _trackAtlas)) return;

            _trackAtlas = value;
            Invalidate();
        }
    }

    public ColorStateValue TrackColors { get; private set; }
    public SpriteStateValue TrackSprites { get; private set; }
    public float WheelCtrlMultiplier { get; set; } = 0.1f;
    public float WheelShiftMultiplier { get; set; } = 10f;
    public float WheelStep { get; set; } = 1f;

    public virtual void ApplyGradientStyle() {
        _trackAtlas = DefaultAtlas.Instance;
        TrackSprites.SuspendAutoRender();
        TrackSprites.SetValues(DefaultAtlas.GradientSlider);
        TrackSprites.ResumeAutoRender(false);
        TrackColors.SuspendAutoRender();
        TrackColors.SetValues(UIColorPalette.White80Percent);
        TrackColors.ResumeAutoRender(false);
        _fillAtlas = null;
        Invalidate();
    }

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        IsPerformingAutoLayout = false;
    }

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    public virtual void SetTheme(ThemeKind value) {
        if (value == _theme) return;

        _theme = value;

        _trackAtlas = DefaultAtlas.Instance;

        TrackSprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);

        TrackColors.SuspendAutoRender();
        if (value == ThemeKind.ClassicLight) {
            TrackColors.SetValues(UIColorPalette.ClassicLightPalette.ElementNormal);
            TrackColors.Disabled = UIColorPalette.ClassicLightPalette.ElementDisabled;
        }
        else {
            TrackColors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlBackground);
            TrackColors.Disabled = UIColorPalette.ClassicDarkPalette.SettingItemControlBackgroundDisabled;
        }

        TrackColors.ResumeAutoRender();

        _fillAtlas = DefaultAtlas.Instance;
        FillSprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
        FillColors.SetValues(value == ThemeKind.ClassicLight ? UIColorPalette.GreenNormal : UIColorPalette.BlueNormal);
        FillColors.Disabled = value == ThemeKind.ClassicLight ? UIColorPalette.GreenDisabled : UIColorPalette.BlueDisabled;
        _thumbAtlas = DefaultAtlas.Instance;
        ThumbSprites.SetValues(DefaultAtlas.Circle);
        ThumbColors.SetValues(new Color32(220, 220, 220, 255));
        ThumbColors.Disabled = new Color32(110, 110, 110, 255);
    }

    public virtual void SetValue(float value, bool notify = true) {
        var v = Mathf.Clamp(value, _minValue, _maxValue);

        if (_stepSize > 0f) {
            var steps = Mathf.Round((v - _minValue) / _stepSize);
            v = _minValue + steps * _stepSize;
            v = Mathf.Clamp(v, _minValue, _maxValue);
        }

        if (Mathf.Approximately(_value, v)) return;

        _value = v;

        if (!Mathf.Approximately(_maxValue, _minValue))
            _normalizedValue = (v - _minValue) / (_maxValue - _minValue);
        else
            _normalizedValue = 0f;

        Invalidate();

        if (notify) {
            NotifyValueChanged(v);
        }
    }

    public virtual void SetValueWithoutNotify(float value) => SetValue(value, false);
    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    protected virtual Vector3 GetThumbOffset() {
        var trackRect = GetTrackRect();
        var thumbSize = _thumbSize * _thumbScale;
        var radius = thumbSize.x * 0.5f;

        if (Direction == Orientation.Horizontal) {
            var centerX = trackRect.xMin + _normalizedValue * trackRect.width;
            var x = centerX - radius;

            var y = trackRect.yMin + (trackRect.height - thumbSize.y) * 0.5f;

            return new Vector2(x, y);
        }
        else {
            var centerY = trackRect.yMin + _normalizedValue * trackRect.height;
            var y = centerY - thumbSize.y * 0.5f;

            var x = trackRect.xMin
                    + (trackRect.width - thumbSize.x) * 0.5f;

            return new Vector2(x, y);
        }
    }

    protected virtual Rect GetTrackRect() => new(
        new Vector2(_padding.Left, _padding.Top),
        size - new Vector2(_padding.Horizontal, _padding.Vertical)
    );

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    protected virtual void RenderFill() {
        if (_fillAtlas == null) return;

        if (_fillRenderData is not null) {
            _fillRenderData.Clear();
        }
        else {
            _fillRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_fillRenderData);
        }

        _fillRenderData.material = _fillAtlas.Material;

        if (!_fillAtlas.TryGetSprite(FillSprites.GetValue(_state), out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _fillAtlas.GetSprite(spriteIndex);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _fillAtlas,
            Color = FillColors.GetValue(_state),
            FillAmount = _normalizedValue,
            FillDirection = _direction == Orientation.Horizontal ? UIFillDirection.Horizontal : UIFillDirection.Vertical,
            Flip = UISpriteFlip.None,
            Offset = _trackRenderPosition,
            UnitsPerPixel = UnitsPerPixel,
            Size = new Vector2(size.x - _padding.Horizontal, size.y - _padding.Vertical),
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_fillRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_fillRenderData, spriteRenderData);
    }

    protected virtual void RenderThumb() {
        if (_thumbAtlas == null) return;

        if (_thumbRenderData is not null) {
            _thumbRenderData.Clear();
        }
        else {
            _thumbRenderData = UIRenderData.Obtain();
            RenderData.Add(_thumbRenderData);
        }

        _thumbRenderData.material = _thumbAtlas.Material;

        if (!_thumbAtlas.TryGetSprite(ThumbSprites.GetValue(_state), out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _thumbAtlas.GetSprite(spriteIndex);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _thumbAtlas,
            SpriteMetadata = spriteMetadata,
            Color = ThumbColors.GetValue(_state),
            Offset = GetThumbOffset(),
            Size = _thumbSize * _thumbScale,
            FillAmount = 1,
            Flip = UISpriteFlip.None,
            UnitsPerPixel = PixelsToUnits()
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_thumbRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_thumbRenderData, spriteRenderData);
    }

    protected virtual void RenderTrack() {
        if (_trackAtlas == null) return;

        if (_trackRenderData is not null) {
            _trackRenderData.Clear();
        }
        else {
            _trackRenderData = UIRenderData.Obtain();
            RenderData.Add(_trackRenderData);
        }

        _trackRenderData.material = _trackAtlas.Material;

        if (!_trackAtlas.TryGetSprite(TrackSprites.GetValue(_state), out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _trackAtlas.GetSprite(spriteIndex);

        var paddedSize = new Vector2(
            size.x - _padding.Horizontal,
            size.y - _padding.Vertical
        );

        SpriteRenderData spriteRenderData = new() {
            Atlas = _trackAtlas,
            Color = TrackColors.GetValue(_state),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = _trackRenderPosition,
            UnitsPerPixel = UnitsPerPixel,
            Size = paddedSize,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_trackRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_trackRenderData, spriteRenderData);
    }

    protected virtual bool TryGetValueFromTrackHit(Vector2 hitPosition, out float value) {
        var trackRect = GetTrackRect();

        if (Direction == Orientation.Horizontal) {
            if (hitPosition.x < trackRect.xMin || hitPosition.x > trackRect.xMax) {
                value = _value;
                return false;
            }

            var nv = Mathf.Clamp01((hitPosition.x - trackRect.xMin) / trackRect.width);
            value = _minValue + nv * (_maxValue - _minValue);
            return true;
        }

        if (Direction == Orientation.Vertical) {
            if (hitPosition.y < trackRect.yMin || hitPosition.y > trackRect.yMax) {
                value = _value;
                return false;
            }

            var nv = Mathf.Clamp01((hitPosition.y - trackRect.yMin) / trackRect.height);
            value = _minValue + nv * (_maxValue - _minValue);
            return true;
        }

        value = _value;
        return false;
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(100, 16);
        TrackSprites = new SpriteStateValue(this);
        TrackColors = new ColorStateValue(this);
        FillSprites = new SpriteStateValue(this);
        FillColors = new ColorStateValue(this);
        ThumbSprites = new SpriteStateValue(this);
        ThumbColors = new ColorStateValue(this);
    }

    protected override void OnMouseDown(UIMouseEventParameter p) {
        if (!p.buttons.IsFlagSet(UIMouseButton.Left)) {
            base.OnMouseMove(p);
            return;
        }

        Focus();
        GetHitPosition(p.ray, out var hitPosition);
        if (!TryGetValueFromTrackHit(hitPosition, out var newValue)) return;

        Value = newValue;
        p.Use();
    }

    protected override void OnMouseMove(UIMouseEventParameter p) {
        if (!p.buttons.IsFlagSet(UIMouseButton.Left)) {
            base.OnMouseMove(p);
            return;
        }

        GetHitPosition(p.ray, out var hitPosition);

        if (TryGetValueFromTrackHit(hitPosition, out var newValue)) {
            Value = newValue;
            p.Use();
        }
    }

    protected override void OnMouseWheel(UIMouseEventParameter p) {
        if (!EnableMouseWheel) return;

        var multiplier = 1f;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            multiplier *= 10f;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            multiplier *= 0.1f;

        Value += WheelStep * p.wheelDelta * multiplier;
        p.Use();
    }

    protected void NotifyValueChanged(float value) => ValueChanged?.Invoke(value);
}