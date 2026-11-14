using ColossalFramework.UI;
using CSLModdingFramework.UI.Utilities;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class RgbColorPicker : Container, IAutoLayout, IThemeable {
    protected static readonly Texture2D BlankTexture = TextureLoader.CreateTexture(16, 16, Color.white);
    protected TextureSpriteView _sVPicker;
    protected TextureSpriteView _hueTextureImage;
    protected SlicedSpriteView _sVSelector;
    protected Slider _hueSlider;
    protected ColorHexField _hexField;
    protected ByteField _rByteField;
    protected ByteField _gByteField;
    protected ByteField _bByteField;
    protected Color _rgbColor = Color.white;
    protected int _autoLayoutSuspendCount;
    protected TextLabel _hexTextLabel;
    protected TextLabel _rTextLabel;
    protected TextLabel _gTextLabel;
    protected TextLabel _bTextLabel;
    protected Button _copyButton;
    protected Button _pasteButton;
    protected ThemeKind _theme;

    public event UIEventHandler<RgbColorPicker, Color> ColorChanged;

    public override bool canFocus => m_CanFocus && isEnabled && isVisible;
    public bool IsPerformingAutoLayout { get; protected set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public Color RgbColor {
        get => _rgbColor;
        set => SetRgbColor(value);
    }

    public ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public virtual void SetTheme(ThemeKind theme) {
        if (theme == _theme) return;
        
        Background.Atlas = DefaultAtlas.Instance;
        _background.SpriteName = DefaultAtlas.SmoothRectangleRadius8;
        switch (theme) {
            case ThemeKind.ClassicLight:
                _background.NormalColor = UIColorPalette.ClassicLightPalette.ElementNormal;
                _background.DisabledColor = UIColorPalette.ClassicLightPalette.ElementDisabled;
                break;
            case ThemeKind.ClassicDark:
                _background.NormalColor = UIColorPalette.ClassicDarkPalette.SettingItemControlBackground;
                _background.DisabledColor = UIColorPalette.ClassicDarkPalette.SettingItemControlBackgroundDisabled;
                break;
        }
    }

    public virtual bool SetRgbColorWithoutNotify(Color value) => SetRgbColor(value, false);

    public virtual bool SetRgbColor(Color value, bool notify = true) {
        if (Mathf.Approximately(value.r, _rgbColor.r) && Mathf.Approximately(value.g, _rgbColor.g) && Mathf.Approximately(value.b, _rgbColor.b)) return false;

        _rgbColor = value;
        _hexField.SetValueWithoutNotify(value);
        _rByteField.SetValueWithoutNotify(((Color32)value).r);
        _gByteField.SetValueWithoutNotify(((Color32)value).g);
        _bByteField.SetValueWithoutNotify(((Color32)value).b);
        Color.RGBToHSV(value, out var hue, out _, out _);
        _hueSlider.SetValueWithoutNotify(hue);
        UpdateSvPicker();
        UpdateSvSelector(_rgbColor);

        if (notify)
            OnRgbColorChanged();

        return true;
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;
        IsPerformingAutoLayout = true;

        _sVPicker.relativePosition = new Vector3(10, 10);
        _hueTextureImage.relativePosition = new Vector3(_sVPicker.relativePosition.x + _sVPicker.width + 12, 10);
        var textLabelY = _hueTextureImage.relativePosition.y + _hueTextureImage.height + 8;
        _hexTextLabel.relativePosition = new Vector3(10, textLabelY);
        var fieldY = _hexTextLabel.relativePosition.y + _hexTextLabel.height + 4;
        _hexField.relativePosition = new Vector3(10, fieldY);
        _rByteField.relativePosition = new Vector3(_hexField.relativePosition.x + _hexField.width + 8, fieldY);
        _gByteField.relativePosition = new Vector3(_rByteField.relativePosition.x + _rByteField.width + 8, fieldY);
        _bByteField.relativePosition = new Vector3(_gByteField.relativePosition.x + _gByteField.width + 8, fieldY);
        _rTextLabel.relativePosition = new Vector3(_rByteField.relativePosition.x, textLabelY);
        _gTextLabel.relativePosition = new Vector3(_gByteField.relativePosition.x, textLabelY);
        _bTextLabel.relativePosition = new Vector3(_bByteField.relativePosition.x, textLabelY);
        _copyButton.relativePosition = new Vector3(10, _bByteField.relativePosition.y + _bByteField.height + 8);
        _pasteButton.relativePosition = new Vector3(_copyButton.relativePosition.x + _copyButton.width + 8, _copyButton.relativePosition.y);

        SetHeightIfChanged(_pasteButton.relativePosition.y + _pasteButton.height + 10);
        SetWidthIfChanged(_hueTextureImage.relativePosition.x + _hueTextureImage.width + 10);

        IsPerformingAutoLayout = false;
    }

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    protected virtual void OnRgbColorChanged() => ColorChanged?.Invoke(this, _rgbColor);

    protected virtual void OnAwake() {
        m_Size = new Vector2(256, 256);
        m_CanFocus = true;

        _sVPicker = AddUIComponent<TextureSpriteView>();
        _sVPicker.SourceMaterial = new Material(Shader.Find("UI/ColorPicker HSB"));
        _sVPicker.SourceTexture = BlankTexture;
        _sVPicker.size = new Vector2(240, 240);
        _sVPicker.canFocus = true;
        _sVPicker.eventMouseDown += OnSvPickerMouseDown;
        _sVPicker.eventMouseMove += OnSvPickerMouseMove;

        _sVSelector = _sVPicker.AddUIComponent<SlicedSpriteView>();
        _sVSelector.size = new Vector2(16, 16);
        _sVSelector.Atlas = DefaultAtlas.Instance;
        _sVSelector.SpriteName = DefaultAtlas.RadioIndicator;
        _sVSelector.relativePosition = Vector3.zero;

        _hueTextureImage = AddUIComponent<TextureSpriteView>();
        _hueTextureImage.SourceMaterial = new Material(Shader.Find("UI/ColorPicker Hue"));
        _hueTextureImage.SourceTexture = BlankTexture;
        _hueTextureImage.size = new Vector2(30, _sVPicker.height);
        _hueTextureImage.Flip = UISpriteFlip.FlipVertical;

        _hueSlider = _hueTextureImage.AddUIComponent<Slider>();
        _hueSlider.size = _hueTextureImage.size;
        _hueSlider.Direction = Orientation.Vertical;
        _hueSlider.relativePosition = Vector2.zero;
        _hueSlider.MinValue = 0f;
        _hueSlider.MaxValue = 1f;
        _hueSlider.StepSize = 0.01f;
        _hueSlider.Value = 0f;
        _hueSlider.SliderValueChanged += OnHueChanged;
        _hueSlider.ThumbAtlas = DefaultAtlas.Instance;
        _hueSlider.ThumbSprites.SetValues(DefaultAtlas.SmoothRectangleRadius4Outline1);
        _hueSlider.ThumbSize = new Vector2(38, 12);
        _hueSlider.EnableMouseWheel = true;

        _hexTextLabel = AddLabel("Hex");
        _rTextLabel = AddLabel("R");
        _gTextLabel = AddLabel("G");
        _bTextLabel = AddLabel("B");

        _hexField = AddUIComponent<ColorHexField>();
        _hexField.size = new Vector2(90, 24);
        _hexField.TextScale = 0.9f;
        _hexField.ValueChanged += OnHexFieldValueChanged;
        _hexField.ApplyStyle(ThemeKind.ClassicLight);
        _hexField.Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
        _hexField.SelectAllOnFocus = true;

        _rByteField = AddByteField();
        _gByteField = AddByteField();
        _bByteField = AddByteField();

        _copyButton = AddUIComponent<Button>();
        _copyButton.size = _hexField.size;
        _copyButton.ApplyStyle(ThemeKind.ClassicLight);
        _copyButton.Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
        _copyButton.Text = "Copy";
        _copyButton.eventClicked += OnCopyClicked;

        _pasteButton = AddUIComponent<Button>();
        _pasteButton.size = _hexField.size;
        _pasteButton.ApplyStyle(ThemeKind.ClassicDark);
        _pasteButton.Text = "Paste";
        _pasteButton.eventClicked += OnPasteClicked;

        InitializeColor();
        PerformAutoLayout();
        SetTheme(ThemeKind.ClassicDark);
    }


    protected virtual ByteField AddByteField() {
        var field = AddUIComponent<ByteField>();
        field.size = new Vector2(56, 24);
        field.TextScale = 0.9f;
        field.MouseWheelEnabled = true;
        field.MinValue = 0;
        field.MaxValue = 255;
        field.UseMinValue = true;
        field.UseMaxValue = true;
        field.StepSize = 5;
        field.ShiftStepSize = 10;
        field.ControlStepSize = 1;
        field.ShowTooltip = true;
        field.SelectAllOnFocus = true;
        field.ApplyStyle(ThemeKind.ClassicLight);
        field.Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
        field.FieldValueChanged += OnRgbFieldValueChanged;
        return field;
    }

    protected virtual void OnHexFieldValueChanged(ColorHexField sender, Color value) {
        _rgbColor = value;

        _rByteField.SetValueWithoutNotify(((Color32)value).r);
        _gByteField.SetValueWithoutNotify(((Color32)value).g);
        _bByteField.SetValueWithoutNotify(((Color32)value).b);
        Color.RGBToHSV(value, out var hue, out _, out _);
        _hueSlider.SetValueWithoutNotify(hue);
        UpdateSvPicker();
        UpdateSvSelector(_rgbColor);

        OnRgbColorChanged();
    }

    protected virtual void OnRgbFieldValueChanged(ValueField sender, byte value) {
        Color32 newColor = _rgbColor;

        if (sender == _rByteField)
            newColor.r = value;
        else if (sender == _gByteField)
            newColor.g = value;
        else if (sender == _bByteField)
            newColor.b = value;
        else
            return;

        _rgbColor = newColor;

        _hexField.SetValueWithoutNotify(newColor);

        Color.RGBToHSV(_rgbColor, out var hue, out _, out _);
        _hueSlider.SetValueWithoutNotify(hue);

        UpdateSvPicker();
        UpdateSvSelector(_rgbColor);

        OnRgbColorChanged();
    }

    protected virtual void OnHueChanged(Slider slider, float hue) {
        Color.RGBToHSV(_rgbColor, out _, out var saturation, out var value);
        _rgbColor = Color.HSVToRGB(hue, saturation, value);
        Color32 color32 = _rgbColor;
        _hexField.SetValueWithoutNotify(_rgbColor);
        _rByteField.SetValueWithoutNotify(color32.r);
        _gByteField.SetValueWithoutNotify(color32.g);
        _bByteField.SetValueWithoutNotify(color32.b);
        UpdateSvPicker();
        UpdateSvSelector(_rgbColor);

        OnRgbColorChanged();
    }

    protected virtual TextLabel AddLabel(string text) {
        var label = AddUIComponent<TextLabel>();
        label.SizeMode = TextSizeMode.FitContent;
        label.TextScale = 0.8f;
        label.Text = text;
        return label;
    }

    public override void Awake() {
        base.Awake();

        OnAwake();
    }

    protected override void OnRebuildRenderData() => _background?.Render();

    private void OnSvPickerMouseDown(UIComponent comp, UIMouseEventParameter p) => UpdateSvSelector(p);

    private void OnSvPickerMouseMove(UIComponent comp, UIMouseEventParameter p) {
        if (p.buttons == UIMouseButton.Left)
            UpdateSvSelector(p);
    }

    private void UpdateSvSelector(UIMouseEventParameter p) {
        if (_sVPicker.GetHitPosition(p.ray, out var a)) {
            _sVSelector.relativePosition = a - _sVSelector.size * 0.5f;
            UpdateSelectedColor();
        }
    }

    private void UpdateSelectedColor() {
        var s = Mathf.Clamp01((_sVSelector.relativePosition.x + _sVSelector.width * 0.5f) / _sVPicker.width);
        var v = Mathf.Clamp01(1f - ((_sVSelector.relativePosition.y + _sVSelector.height * 0.5f) / _sVPicker.height));

        Color.RGBToHSV(_rgbColor, out var hue, out _, out _);
        _rgbColor = Color.HSVToRGB(hue, s, v);

        _hexField.SetValueWithoutNotify(_rgbColor);
        _rByteField.SetValueWithoutNotify(((Color32)_rgbColor).r);
        _gByteField.SetValueWithoutNotify(((Color32)_rgbColor).g);
        _bByteField.SetValueWithoutNotify(((Color32)_rgbColor).b);

        OnRgbColorChanged();
    }

    private void UpdateSvPicker() {
        if (_sVPicker.RenderMaterial != null) {
            var hueColor = Color.HSVToRGB(_hueSlider.Value, 1f, 1f);
            _sVPicker.RenderMaterial.color = hueColor;
        }
    }

    private void UpdateSvSelector(Color rgbColor) {
        Color.RGBToHSV(rgbColor, out _, out var s, out var v);
        var centerX = s * _sVPicker.width;
        var centerY = (1f - v) * _sVPicker.height;
        var x = centerX - _sVSelector.width * 0.5f;
        var y = centerY - _sVSelector.height * 0.5f;

        x = Mathf.Clamp(
            x,
            -_sVSelector.width * 0.5f,
            _sVPicker.width - _sVSelector.width * 0.5f
        );

        y = Mathf.Clamp(
            y,
            -_sVSelector.height * 0.5f,
            _sVPicker.height - _sVSelector.height * 0.5f
        );

        _sVSelector.relativePosition = new Vector3(x, y);
    }

    private void InitializeColor() {
        Color.RGBToHSV(_rgbColor, out var hue, out _, out _);
        _hueSlider.SetValueWithoutNotify(hue);
        _hexField.SetValueWithoutNotify(_rgbColor);
        var color32 = (Color32)_rgbColor;
        _rByteField.SetValueWithoutNotify(color32.r);
        _gByteField.SetValueWithoutNotify(color32.g);
        _bByteField.SetValueWithoutNotify(color32.b);
        UpdateSvPicker();
        UpdateSvSelector(_rgbColor);
    }

    private void OnCopyClicked(UIComponent component, UIMouseEventParameter eventParam) => ColorClipboard.Color = _rgbColor;

    private void OnPasteClicked(UIComponent component, UIMouseEventParameter eventParam) {
        if (ColorClipboard.HasValue) {
            SetRgbColor(ColorClipboard.Color);
        }
    }
}