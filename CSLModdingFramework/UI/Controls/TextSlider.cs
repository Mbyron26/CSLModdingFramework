using System;
using System.Globalization;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class TextSlider : SliderBase {
    protected DynamicFont _font = FontManager.OpenSansRegularFont;
    protected int _spacing = 6;
    protected string _text;
    protected float _textCharacterSpacing;
    protected UIRenderData _textRenderData;
    protected float _textScale = 1;
    protected int _trackHeight = 16;
    protected string _valueFormat;
    protected Func<float, string> _valueFormatter;
    protected string _valueText;
    private float _textRenderMaxWidth;
    private Vector3 _textRenderPosition;
    private float _textValueRenderMaxWidth;
    private Vector3 _valueTextRenderPosition;

    public event UIEventHandler<TextSlider, float> SliderValueChanged;

    public virtual float TextCharacterSpacing {
        get => _textCharacterSpacing;
        set {
            value = Mathf.Max(0, value);
            if (Mathf.Approximately(value, _textCharacterSpacing)) return;

            _textCharacterSpacing = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float TextScale {
        get => _textScale;
        set {
            value = Mathf.Max(0.1f, value);
            if (Mathf.Approximately(_textScale, value)) return;

            _textScale = value;
            InvalidateAutoLayout();
        }
    }

    public virtual string ValueFormat {
        get => _valueFormat;
        set {
            if (string.Equals(value, _valueFormat)) return;

            _valueFormat = value;
        }
    }

    public override Orientation Direction {
        get => _direction;
        set { }
    }

    public DynamicFont Font {
        get => _font;
        set {
            if (ReferenceEquals(value, _font)) return;

            _font = value;
            Invalidate();
        }
    }

    public int Spacing {
        get => _spacing;
        set {
            if (value == _spacing) return;

            _spacing = value;
            InvalidateAutoLayout();
        }
    }

    public string Text {
        get => _text;
        set {
            if (string.Equals(value, _text)) return;

            _text = value;
            InvalidateAutoLayout();
        }
    }

    public ColorStateValue TextColors { get; private set; }

    public int TrackHeight {
        get => _trackHeight;
        set {
            if (value == _trackHeight) return;

            _trackHeight = value;
            InvalidateAutoLayout();
        }
    }

    public Func<float, string> ValueFormatter {
        get => _valueFormatter;
        set {
            _valueFormatter = value;
            _valueText = FormatValue(_value);
            InvalidateAutoLayout();
        }
    }

    public ColorStateValue ValueTextColors { get; private set; }

    public virtual DynamicFontRenderer RentTextRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.CharacterSpacing = _textCharacterSpacing;
        fontRenderer.WordWrap = true;
        fontRenderer.MaxSize = new Vector2(_textRenderMaxWidth, 4096);
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _textScale;
        fontRenderer.LayoutOffset = _textRenderPosition;
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = false;
        fontRenderer.DefaultColor = TextColors.GetValue(_state);
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    public virtual DynamicFontRenderer RentValueTextRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.CharacterSpacing = _textCharacterSpacing;
        fontRenderer.WordWrap = true;
        fontRenderer.MaxSize = new Vector2(_textValueRenderMaxWidth, 4096);
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _textScale;
        fontRenderer.LayoutOffset = _valueTextRenderPosition;
        fontRenderer.TextAlign = UIHorizontalAlignment.Right;
        fontRenderer.ProcessMarkup = false;
        fontRenderer.DefaultColor = TextColors.GetValue(_state);
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    protected virtual string FormatValue(float value) {
        if (_valueFormatter != null)
            return _valueFormatter(value);

        if (_valueFormat != null)
            return value.ToString(_valueFormat);

        return value.ToString(CultureInfo.InvariantCulture);
    }

    protected virtual void RenderText() {
        if (Font is not { IsValid: true }) return;

        if (_textRenderData != null) {
            _textRenderData.Clear();
        }
        else {
            _textRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_textRenderData);
        }

        using (var textRenderer = RentValueTextRenderer()) {
            textRenderer.Render(_valueText, _textRenderData);
        }

        if (!string.IsNullOrEmpty(_text)) {
            using (var textRenderer = RentTextRenderer()) {
                textRenderer.Render(_text, _textRenderData);
            }
        }
    }

    public override void Awake() {
        _valueText = _value.ToString(CultureInfo.InvariantCulture);

        base.Awake();

        m_Size = new Vector2(100, 38);
        TextColors = new ColorStateValue(this);
        TextColors.SetValues(UIColorPalette.ClassicDarkPalette.HeaderTextColors);
        ValueTextColors = new ColorStateValue(this);
    }

    public override void OnDisable() {
        base.OnDisable();

        UnityEngine.Font.textureRebuilt -= OnUnityFontTextureRebuilt;
    }

    public override void OnEnable() {
        base.OnEnable();

        UnityEngine.Font.textureRebuilt += OnUnityFontTextureRebuilt;
    }

    public override void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        _textRenderMaxWidth = _textValueRenderMaxWidth = width - _padding.Horizontal;

        float allTextHeight = 0;
        using (var valueTextRenderer = RentValueTextRenderer()) {
            var valueTextSize = valueTextRenderer.GetRenderedSize(_valueText);
            _valueTextRenderPosition = new Vector3(_padding.Left, _padding.Top);
            allTextHeight = Mathf.Max(allTextHeight, valueTextSize.y);
        }

        if (!string.IsNullOrEmpty(_text)) {
            using (var textRenderer = RentTextRenderer()) {
                var textSize = textRenderer.GetRenderedSize(_text);
                _textRenderPosition = new Vector3(_padding.Left, _padding.Top);
                allTextHeight = Mathf.Max(allTextHeight, textSize.y);
            }
        }

        _trackRenderPosition = new Vector3(_padding.Left, _padding.Top + allTextHeight + _spacing);

        SetHeightIfChanged(_padding.Vertical + _trackHeight + allTextHeight + _spacing);

        IsPerformingAutoLayout = false;
    }

    public override void SetValue(float value, bool notify = true) {
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

        _valueText = FormatValue(_value);
        Invalidate();

        if (!notify) return;

        SliderValueChanged?.Invoke(this, v);
        NotifyValueChanged(v);
    }

    protected override Vector3 GetThumbOffset() {
        var trackRect = GetTrackRect();
        var thumbSize = _thumbSize * _thumbScale;
        var radius = thumbSize.x * 0.5f;

        var centerX = trackRect.xMin + _normalizedValue * trackRect.width;
        var x = centerX - radius;

        var y = trackRect.yMin + (trackRect.height - thumbSize.y) * 0.5f;

        return new Vector2(x, y);
    }

    protected override Rect GetTrackRect() => new(
        new Vector2(_trackRenderPosition.x, _trackRenderPosition.y),
        new Vector2(width - _padding.Horizontal, _trackHeight)
    );

    protected override void OnKeyDown(UIKeyEventParameter p) {
        if (!EnableArrowKeys || p.used) {
            base.OnKeyDown(p);
            return;
        }

        var delta = 0f;

        if (p.keycode == KeyCode.LeftArrow)
            delta = -ArrowKeyStep;

        else if (p.keycode == KeyCode.RightArrow)
            delta = ArrowKeyStep;

        if (delta != 0f) {
            var multiplier = 1f;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                multiplier *= ArrowShiftMultiplier;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                multiplier *= ArrowCtrlMultiplier;

            Value += delta * multiplier;
            p.Use();
            return;
        }

        base.OnKeyDown(p);
    }

    protected override void OnRebuildRenderData() {
        if (IsAutoLayoutSuspended) return;

        RenderTrack();
        RenderFill();
        RenderThumb();
        RenderText();
    }

    protected override void RenderFill() {
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
            Size = new Vector2(width - _padding.Horizontal, _trackHeight),
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_fillRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_fillRenderData, spriteRenderData);
    }

    protected override void RenderTrack() {
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

        SpriteRenderData spriteRenderData = new() {
            Atlas = _trackAtlas,
            Color = TrackColors.GetValue(_state),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = _trackRenderPosition,
            UnitsPerPixel = UnitsPerPixel,
            Size = new Vector2(width - _padding.Horizontal, _trackHeight),
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_trackRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_trackRenderData, spriteRenderData);
    }

    protected override bool TryGetValueFromTrackHit(Vector2 hitPosition, out float value) {
        var trackRect = GetTrackRect();

        if (hitPosition.x < trackRect.xMin || hitPosition.x > trackRect.xMax) {
            value = _value;
            return false;
        }

        var nv = Mathf.Clamp01((hitPosition.x - trackRect.xMin) / trackRect.width);
        value = _minValue + nv * (_maxValue - _minValue);
        return true;
    }

    private void OnUnityFontTextureRebuilt(Font font) => Invalidate();
}