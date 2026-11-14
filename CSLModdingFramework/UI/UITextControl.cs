using System;
using ColossalFramework.UI;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

[Obsolete("Use TextControlBase instead.")]
public abstract class UITextControl : UIInteractiveControl {
    protected UIRenderData _textRenderData;
    protected UITextureAtlas _textAtlas;
    protected UIFont _font;
    protected string _text = string.Empty;
    protected float _textScale = 1f;
    protected EdgeInsets _textPadding;
    protected UITextScaleMode _textScaleMode;
    protected int _characterSpacing;
    protected bool _processMarkup;
    protected bool _useOutline;
    protected int _outlineSize = 1;
    protected Color32 _outlineColor = Color.black;
    protected bool _useTextGradient;
    protected Color32 _gradientBottomNormalColor = Color.white;
    protected Color32 _gradientBottomDisabledColor = Color.white;
    protected bool _useDropShadow;
    protected Color32 _dropShadowColor = Color.black;
    protected Vector2 _dropShadowOffset;
    protected Vector2 _startSize;
    protected bool _colorizeSprites;
    protected UIHorizontalAlignment _textHorizontalAlignment = UIHorizontalAlignment.Center;
    protected UIVerticalAlignment _textVerticalAlignment = UIVerticalAlignment.Middle;
    protected bool _wordWrap;
    private bool _isFontCallbackAssigned;

    public ColorStateValue TextColors { get; private set; }

    public UITextureAtlas TextAtlas {
        get => _textAtlas ??= GetUIView()?.defaultAtlas;
        set {
            if (Equals(value, _textAtlas)) return;

            _textAtlas = value;
            Invalidate();
        }
    }

    public UIFont Font {
        get => _font ??= GetUIView()?.defaultFont;
        set {
            if (value == _font) return;
            UnbindTextureRebuildCallback();
            _font = value;
            BindTextureRebuildCallback();
            Invalidate();
        }
    }

    public virtual string Text {
        get => _text;
        set {
            if (value == _text) return;
            UIFontManager.Invalidate(Font);
            _text = value;
            Invalidate();
        }
    }

    public float TextScale {
        get => _textScale;
        set {
            value = Mathf.Max(0.1f, value);
            if (Mathf.Approximately(_textScale, value)) return;

            UIFontManager.Invalidate(Font);
            _textScale = value;
            Invalidate();
        }
    }

    public EdgeInsets TextPadding {
        get => _textPadding;
        set {
            if (_textPadding.Equals(value)) return;

            _textPadding = value;
            Invalidate();
        }
    }

    public UITextScaleMode TextScaleMode {
        get => _textScaleMode;
        set {
            if (value == _textScaleMode) return;

            _textScaleMode = value;
            Invalidate();
        }
    }

    public int CharacterSpacing {
        get => _characterSpacing;
        set {
            value = Mathf.Max(0, value);
            if (value == _characterSpacing) return;

            _characterSpacing = value;
            Invalidate();
        }
    }

    public bool ProcessMarkup {
        get => _processMarkup;
        set {
            if (value == _processMarkup) return;

            _processMarkup = value;
            Invalidate();
        }
    }

    public bool UseOutline {
        get => _useOutline;
        set {
            if (value == _useOutline) return;

            _useOutline = value;
            Invalidate();
        }
    }

    public int OutlineSize {
        get => _outlineSize;
        set {
            value = Mathf.Max(0, value);
            if (value == _outlineSize) return;

            _outlineSize = value;
            Invalidate();
        }
    }

    public Color32 OutlineColor {
        get => _outlineColor;
        set {
            if (_outlineColor.EqualsColor(value)) return;

            _outlineColor = value;
            Invalidate();
        }
    }

    public bool UseTextGradient {
        get => _useTextGradient;
        set {
            if (value == _useTextGradient) return;

            _useTextGradient = value;
            Invalidate();
        }
    }

    public Color32 GradientBottomNormalColor {
        get => _gradientBottomNormalColor;
        set {
            if (_gradientBottomNormalColor.EqualsColor(value)) return;

            _gradientBottomNormalColor = value;
            OnColorChanged();
        }
    }

    public Color32 GradientBottomDisabledColor {
        get => _gradientBottomDisabledColor;
        set {
            if (_gradientBottomDisabledColor.EqualsColor(value)) return;

            _gradientBottomDisabledColor = value;
            OnColorChanged();
        }
    }

    public bool UseDropShadow {
        get => _useDropShadow;
        set {
            if (value == _useDropShadow) return;

            _useDropShadow = value;
            Invalidate();
        }
    }

    public Color32 DropShadowColor {
        get => _dropShadowColor;
        set {
            if (_dropShadowColor.EqualsColor(value)) return;

            _dropShadowColor = value;
            Invalidate();
        }
    }

    public Vector2 DropShadowOffset {
        get => _dropShadowOffset;
        set {
            if (value == _dropShadowOffset) return;

            _dropShadowOffset = value;
            Invalidate();
        }
    }

    public bool ColorizeSprites {
        get => _colorizeSprites;
        set {
            if (value == _colorizeSprites) return;

            _colorizeSprites = value;
            Invalidate();
        }
    }

    public UIHorizontalAlignment TextHorizontalAlignment {
        get => _textHorizontalAlignment;
        set {
            if (value == _textHorizontalAlignment) return;

            _textHorizontalAlignment = value;
            Invalidate();
        }
    }

    public virtual UIVerticalAlignment TextVerticalAlignment {
        get => _textVerticalAlignment;
        set {
            if (value == _textVerticalAlignment) return;

            _textVerticalAlignment = value;
            Invalidate();
        }
    }

    public bool WordWrap {
        get => _wordWrap;
        set {
            if (value == _wordWrap) return;

            _wordWrap = value;
            Invalidate();
        }
    }

    public virtual void UpdateFontInfo() => RequestCharacterInfo();

    protected virtual Color32 GetTextColor() => TextColors.GetValue(_state);

    protected virtual Color32 GetGradientBottomColorForState() => isEnabled ? GradientBottomNormalColor : GradientBottomDisabledColor;

    protected virtual void RenderText() {
        if (Font == null || !Font.isValid) return;

        if (_textRenderData != null) {
            _textRenderData.Clear();
        }
        else {
            _textRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_textRenderData);
        }

        _textRenderData.material = TextAtlas.material;

        using var uiFontRenderer = ObtainTextRenderer();
        if (uiFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer) {
            dynamicFontRenderer.spriteAtlas = TextAtlas;
            dynamicFontRenderer.spriteBuffer = _textRenderData;
        }

        uiFontRenderer.Render(_text, _textRenderData);
    }

    protected virtual Vector3 GetVertAlignOffset(UIFontRenderer fontRenderer, string text) {
        var unitScale = PixelsToUnits();
        var textSize = fontRenderer.MeasureString(text) * unitScale;
        var vectorOffset = fontRenderer.vectorOffset;
        var availableHeight = (height - TextPadding.Vertical) * unitScale;

        if (textSize.y >= availableHeight)
            return vectorOffset;

        var verticalGap = availableHeight - textSize.y;
        switch (TextVerticalAlignment) {
            case UIVerticalAlignment.Middle:
                vectorOffset.y -= verticalGap * 0.5f;
                break;
            case UIVerticalAlignment.Bottom:
                vectorOffset.y -= verticalGap;
                break;
        }

        return vectorOffset;
    }

    protected virtual float GetTextScaleMultiplier() {
        if (TextScaleMode == UITextScaleMode.None || !Application.isPlaying) return 1f;

        if (TextScaleMode == UITextScaleMode.ScreenResolution) return (float)Screen.height / GetUIView().fixedHeight;

        return size.y / _startSize.y;
    }

    protected virtual void RequestCharacterInfo() {
        var dynamicFont = Font as UIDynamicFont;
        if (dynamicFont == null) return;

        if (!UIFontManager.IsDirty(Font) || string.IsNullOrEmpty(Text)) return;

        var num = TextScale * GetTextScaleMultiplier();
        var fontSize = Mathf.CeilToInt(Font.size * num);
        dynamicFont.AddCharacterRequest(Text, fontSize, FontStyle.Normal);
    }

    public virtual UIFontRenderer ObtainTextRenderer() {
        var contentArea = size - new Vector2(TextPadding.Horizontal, TextPadding.Vertical);

        var num = PixelsToUnits();
        var vectorOffset = (pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + new Vector3(TextPadding.Left, -TextPadding.Top)) * num;

        var uiFontRenderer = Font.ObtainRenderer();
        uiFontRenderer.wordWrap = _wordWrap;
        uiFontRenderer.multiLine = true;
        uiFontRenderer.maxSize = contentArea;
        uiFontRenderer.pixelRatio = num;
        uiFontRenderer.textScale = TextScale;
        uiFontRenderer.vectorOffset = vectorOffset;
        uiFontRenderer.textAlign = TextHorizontalAlignment;
        uiFontRenderer.processMarkup = _processMarkup;
        uiFontRenderer.defaultColor = GetTextColor();
        uiFontRenderer.bottomColor = _useTextGradient ? new Color32?(GetGradientBottomColorForState()) : null;
        uiFontRenderer.overrideMarkupColors = false;
        uiFontRenderer.opacity = CalculateOpacity();
        uiFontRenderer.shadow = _useDropShadow;
        uiFontRenderer.shadowColor = _dropShadowColor;
        uiFontRenderer.shadowOffset = _dropShadowOffset;
        uiFontRenderer.outline = _useOutline;
        uiFontRenderer.outlineSize = _outlineSize;
        uiFontRenderer.outlineColor = _outlineColor;
        if (TextVerticalAlignment != UIVerticalAlignment.Top)
            uiFontRenderer.vectorOffset = GetVertAlignOffset(uiFontRenderer, _text);

        return uiFontRenderer;
    }

    public override void Awake() {
        base.Awake();
        _startSize = size;
        TextColors = new ColorStateValue(this);
    }

    public override void OnEnable() {
        base.OnEnable();
        BindTextureRebuildCallback();
    }

    public override void OnDisable() {
        base.OnDisable();
        UnbindTextureRebuildCallback();
    }

    public Vector2 MeasureIntrinsicSize() {
        if (string.IsNullOrEmpty(_text)) return Vector2.zero;

        var uiFontRenderer = Font.ObtainRenderer();
        uiFontRenderer.wordWrap = false;
        uiFontRenderer.multiLine = false;
        uiFontRenderer.maxSize = new Vector2(4096, 4096);
        uiFontRenderer.pixelRatio = PixelsToUnits();
        uiFontRenderer.textScale = TextScale;
        uiFontRenderer.vectorOffset = Vector3.zero;
        uiFontRenderer.textAlign = UIHorizontalAlignment.Left;
        uiFontRenderer.processMarkup = _processMarkup;
        uiFontRenderer.defaultColor = UIColorPalette.White;
        uiFontRenderer.bottomColor = UIColorPalette.White;
        uiFontRenderer.overrideMarkupColors = false;
        return uiFontRenderer.MeasureString(_text);
    }

    private void BindTextureRebuildCallback() {
        if (_isFontCallbackAssigned || Font == null) return;

        if (Font as UIDynamicFont == null) return;

        UnityEngine.Font.textureRebuilt += OnFontTextureRebuilt;
        _isFontCallbackAssigned = true;
    }

    private void UnbindTextureRebuildCallback() {
        if (!_isFontCallbackAssigned || Font == null) return;

        if (Font as UIDynamicFont != null)
            UnityEngine.Font.textureRebuilt -= OnFontTextureRebuilt;

        _isFontCallbackAssigned = false;
    }

    private void OnFontTextureRebuilt(Font font) {
        RequestCharacterInfo();
        Invalidate();
    }
}