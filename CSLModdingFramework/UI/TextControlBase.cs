using System;
using ColossalFramework.UI;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

[Obsolete("Use TextControl instead")]
public abstract class TextControlBase : UIInteractiveControl, IAutoLayout {
    protected UIRenderData _textRenderData;
    protected UITextureAtlas _textAtlas;
    protected UIFont _font;
    protected string _text = string.Empty;
    protected float _textScale = 1f;
    protected EdgeInsets _textPadding;
    protected int _characterSpacing;
    protected bool _processMarkup;
    protected bool _useOutline;
    protected int _outlineSize = 1;
    protected Color32 _outlineColor = UIColorPalette.Black;
    protected bool _useTextGradient;
    protected Color32 _gradientBottomNormalColor = UIColorPalette.White;
    protected Color32 _gradientBottomDisabledColor = UIColorPalette.White;
    protected bool _useDropShadow;
    protected Color32 _dropShadowColor = UIColorPalette.Black;
    protected Vector2 _dropShadowOffset;
    protected bool _colorizeSprites;
    protected UIHorizontalAlignment _textHorizontalAlignment = UIHorizontalAlignment.Center;
    protected UIVerticalAlignment _textVerticalAlignment = UIVerticalAlignment.Middle;
    protected bool _wordWrap;
    protected int _tabSize = 48;
    protected TextSizeMode _sizeMode;
    protected Vector2 _textAlignOffset;
    protected float _textRenderWidth;
    protected float _textRenderHeight;
    protected Size _minSize;
    protected Size _maxSize;
    protected int _autoLayoutSuspendCount;

    public ColorStateValue TextColors { get; private set; }
    public UITextureAtlas TextAtlas => _textAtlas ??= GetUIView()?.defaultAtlas;
    public UIFont Font => _font ??= GetUIView()?.defaultFont;
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;
    public bool IsPerformingAutoLayout { get; protected internal set; }

    public virtual string Text {
        get => _text;
        set => SetText(value);
    }

    public virtual Size MinSize {
        get => _minSize;
        set {
            if (value.Approximately(_minSize) || value > _maxSize) return;

            _minSize = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Size MaxSize {
        get => _maxSize;
        set {
            if (value.Approximately(_maxSize) || value < _minSize) return;

            _maxSize = value;
            InvalidateAutoLayout();
        }
    }

    public virtual TextSizeMode SizeMode {
        get => _sizeMode;
        set {
            if (value == _sizeMode) return;

            _sizeMode = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float TextScale {
        get => _textScale;
        set {
            value = Mathf.Max(0.1f, value);
            if (Mathf.Approximately(_textScale, value)) return;

            UIFontManager.Invalidate(Font);
            _textScale = value;
            InvalidateAutoLayout();
        }
    }

    public virtual EdgeInsets TextPadding {
        get => _textPadding;
        set {
            if (_textPadding.Equals(value)) return;

            _textPadding = value;
            InvalidateAutoLayout();
        }
    }

    public virtual int CharacterSpacing {
        get => _characterSpacing;
        set {
            value = Mathf.Max(0, value);
            if (value == _characterSpacing) return;

            _characterSpacing = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool ProcessMarkup {
        get => _processMarkup;
        set {
            if (value == _processMarkup) return;

            _processMarkup = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool UseOutline {
        get => _useOutline;
        set {
            if (value == _useOutline) return;

            _useOutline = value;
            InvalidateAutoLayout();
        }
    }

    public virtual int OutlineSize {
        get => _outlineSize;
        set {
            value = Mathf.Max(0, value);
            if (value == _outlineSize) return;

            _outlineSize = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Color32 OutlineColor {
        get => _outlineColor;
        set {
            if (_outlineColor.EqualsColor(value)) return;

            _outlineColor = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool UseTextGradient {
        get => _useTextGradient;
        set {
            if (value == _useTextGradient) return;

            _useTextGradient = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Color32 GradientBottomNormalColor {
        get => _gradientBottomNormalColor;
        set {
            if (_gradientBottomNormalColor.EqualsColor(value)) return;

            _gradientBottomNormalColor = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Color32 GradientBottomDisabledColor {
        get => _gradientBottomDisabledColor;
        set {
            if (_gradientBottomDisabledColor.EqualsColor(value)) return;

            _gradientBottomDisabledColor = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool UseDropShadow {
        get => _useDropShadow;
        set {
            if (value == _useDropShadow) return;

            _useDropShadow = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Color32 DropShadowColor {
        get => _dropShadowColor;
        set {
            if (_dropShadowColor.EqualsColor(value)) return;

            _dropShadowColor = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Vector2 DropShadowOffset {
        get => _dropShadowOffset;
        set {
            if (value == _dropShadowOffset) return;

            _dropShadowOffset = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool ColorizeSprites {
        get => _colorizeSprites;
        set {
            if (value == _colorizeSprites) return;

            _colorizeSprites = value;
            InvalidateAutoLayout();
        }
    }

    public virtual UIHorizontalAlignment TextHorizontalAlignment {
        get => _textHorizontalAlignment;
        set {
            if (value == _textHorizontalAlignment) return;

            _textHorizontalAlignment = value;
            InvalidateAutoLayout();
        }
    }

    public virtual UIVerticalAlignment TextVerticalAlignment {
        get => _textVerticalAlignment;
        set {
            if (value == _textVerticalAlignment) return;

            _textVerticalAlignment = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool WordWrap {
        get => _wordWrap;
        set {
            if (value == _wordWrap) return;

            _wordWrap = value;
            InvalidateAutoLayout();
        }
    }

    public virtual int TabSize {
        get => _tabSize;
        set {
            value = Mathf.Max(0, value);
            if (value == _tabSize) return;

            _tabSize = value;
            InvalidateAutoLayout();
        }
    }

    public virtual UIFontRenderer ObtainTextRenderer() {
        var pixelsToUnits = PixelsToUnits();
        var vectorOffset = _textAlignOffset.Scale(1, -1) * pixelsToUnits;

        var uiFontRenderer = Font.ObtainRenderer();
        uiFontRenderer.wordWrap = _wordWrap;
        uiFontRenderer.multiLine = true;
        uiFontRenderer.maxSize = new Vector2(_textRenderWidth, _textRenderHeight);
        uiFontRenderer.pixelRatio = pixelsToUnits;
        uiFontRenderer.textScale = _textScale;
        uiFontRenderer.vectorOffset = vectorOffset;
        uiFontRenderer.textAlign = _textHorizontalAlignment;
        uiFontRenderer.processMarkup = _processMarkup;
        uiFontRenderer.defaultColor = GetTextColor();
        uiFontRenderer.bottomColor = _useTextGradient ? GetGradientBottomColor() : null;
        uiFontRenderer.overrideMarkupColors = false;
        uiFontRenderer.opacity = 1f;
        uiFontRenderer.shadow = _useDropShadow;
        uiFontRenderer.shadowColor = _dropShadowColor;
        uiFontRenderer.shadowOffset = _dropShadowOffset;
        uiFontRenderer.outline = _useOutline;
        uiFontRenderer.outlineSize = _outlineSize;
        uiFontRenderer.outlineColor = _outlineColor;
        return uiFontRenderer;
    }

    public virtual bool SetText(string text, bool notify = true) {
        if (string.Equals(text, _text)) return false;

        _text = text;
        InvalidateAutoLayout();

        if (notify)
            OnTextChanged();

        return true;
    }

    public virtual void PerformAutoLayout() {
        IsPerformingAutoLayout = true;
        var textIntrinsicSize = MeasureTextSize();

        float finalWidth;
        float finalHeight;
        float finalTextOffsetX;
        float finalTextOffsetY;
        if (_sizeMode == TextSizeMode.FitContent) {
            if (_maxSize.IsNearlyZero())
                _maxSize = new Size(4096, 4096);

            var intrinsicWidth = textIntrinsicSize.x;
            var intrinsicHeight = textIntrinsicSize.y;

            var paddedWidth = intrinsicWidth + _textPadding.Horizontal;
            var paddedHeight = intrinsicHeight + _textPadding.Vertical;

            var minWidth = _minSize.Width;
            var minHeight = _minSize.Height;
            var maxWidth = _maxSize.Width;
            var maxHeight = _maxSize.Height;

            if (paddedWidth <= minWidth) {
                _textRenderWidth = intrinsicWidth;

                if (_textHorizontalAlignment == UIHorizontalAlignment.Left) {
                    finalTextOffsetX = _textPadding.Left;
                }
                else if (_textHorizontalAlignment == UIHorizontalAlignment.Right) {
                    finalTextOffsetX = minWidth - _textRenderWidth - _textPadding.Right;
                }
                else {
                    finalTextOffsetX = (minWidth - _textRenderWidth) / 2;
                }

                if (paddedHeight < minHeight) {
                    _textRenderHeight = minHeight - _textPadding.Vertical;
                    finalHeight = minHeight;
                    finalTextOffsetY = _textPadding.Top;
                }
                else if (paddedHeight > maxHeight) {
                    _textRenderHeight = maxHeight - _textPadding.Vertical;
                    finalHeight = maxHeight;

                    if (_textVerticalAlignment == UIVerticalAlignment.Top) {
                        finalTextOffsetY = _textPadding.Top;
                    }
                    else if (_textVerticalAlignment == UIVerticalAlignment.Bottom) {
                        finalTextOffsetY = finalHeight - _textRenderHeight - _textPadding.Bottom;
                    }
                    else {
                        finalTextOffsetY = (finalHeight - _textRenderHeight) / 2;
                    }
                }
                else {
                    _textRenderHeight = intrinsicHeight;
                    finalHeight = _textRenderHeight + _textPadding.Vertical;
                    finalTextOffsetY = _textPadding.Top;
                }

                finalWidth = minWidth;
            }
            else if (paddedWidth >= maxWidth) {
                _textRenderWidth = maxWidth - _textPadding.Horizontal;
                var newTextHeight = MeasureTextSize(_textRenderWidth, allowWordWrap: _wordWrap).y;

                finalTextOffsetX = _textPadding.Left;

                if (newTextHeight <= minHeight) {
                    _textRenderHeight = minHeight - _textPadding.Vertical;
                    finalHeight = minHeight;
                    if (_textVerticalAlignment == UIVerticalAlignment.Top) {
                        finalTextOffsetY = _textPadding.Top;
                    }
                    else if (_textVerticalAlignment == UIVerticalAlignment.Bottom) {
                        finalTextOffsetY = finalHeight - _textRenderHeight - _textPadding.Bottom;
                    }
                    else {
                        finalTextOffsetY = (finalHeight - _textRenderHeight) / 2;
                    }
                }
                else if (newTextHeight >= maxHeight) {
                    _textRenderHeight = maxHeight - _textPadding.Vertical;
                    finalHeight = maxHeight;
                    finalTextOffsetY = _textPadding.Top;
                }
                else {
                    _textRenderHeight = newTextHeight;
                    finalHeight = _textRenderHeight + _textPadding.Vertical;
                    finalTextOffsetY = _textPadding.Top;
                }

                finalWidth = maxWidth;
            }
            else {
                _textRenderWidth = intrinsicWidth;
                _textRenderHeight = intrinsicHeight;
                finalWidth = _textRenderWidth + _textPadding.Horizontal;
                finalHeight = _textRenderHeight + _textPadding.Vertical;
                finalTextOffsetX = _textPadding.Left;
                finalTextOffsetY = _textPadding.Top;
            }
        }
        else {
            finalWidth = m_Size.x;
            finalHeight = m_Size.y;

            var textSize = MeasureTextSize(finalWidth - _textPadding.Horizontal, finalHeight > 4096 ? finalHeight : 4096, _wordWrap);

            _textRenderWidth = textSize.x;
            _textRenderHeight = textSize.y;

            if (_textHorizontalAlignment == UIHorizontalAlignment.Left) {
                finalTextOffsetX = _textPadding.Left;
            }
            else if (_textHorizontalAlignment == UIHorizontalAlignment.Right) {
                finalTextOffsetX = finalWidth - _textRenderWidth - _textPadding.Right;
            }
            else {
                finalTextOffsetX = (finalWidth - _textRenderWidth) / 2;
            }

            if (_textVerticalAlignment == UIVerticalAlignment.Top) {
                finalTextOffsetY = _textPadding.Top;
            }
            else if (_textVerticalAlignment == UIVerticalAlignment.Bottom) {
                finalTextOffsetY = finalHeight - _textRenderHeight - _textPadding.Bottom;
            }
            else {
                finalTextOffsetY = (finalHeight - _textRenderHeight) / 2;
            }
        }

        SetSizeInternal(new Vector2(finalWidth, finalHeight));
        _textAlignOffset = new Vector2(finalTextOffsetX, finalTextOffsetY);
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

    public virtual Vector2 MeasureTextSize(float maxWidth = 4096, float maxHeight = 4096, bool allowWordWrap = false) {
        if (string.IsNullOrEmpty(_text)) return Vector2.zero;

        using var uiFontRenderer = Font.ObtainRenderer();
        uiFontRenderer.wordWrap = allowWordWrap;
        uiFontRenderer.multiLine = false;
        uiFontRenderer.maxSize = new Vector2(maxWidth, maxHeight);
        uiFontRenderer.pixelRatio = PixelsToUnits();
        uiFontRenderer.textScale = _textScale;
        uiFontRenderer.vectorOffset = Vector3.zero;
        uiFontRenderer.textAlign = UIHorizontalAlignment.Left;
        uiFontRenderer.processMarkup = _processMarkup;
        uiFontRenderer.defaultColor = UIColorPalette.White;
        uiFontRenderer.bottomColor = UIColorPalette.White;
        uiFontRenderer.overrideMarkupColors = false;
        return uiFontRenderer.MeasureString(_text);
    }

    protected virtual void RenderText() {
        if (Font == null || !Font.isValid) return;

        if (_textRenderData != null) {
            _textRenderData.Clear();
        }
        else {
            _textRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_textRenderData);
        }

        if (isLayoutSuspended || string.IsNullOrEmpty(_text)) return;

        _textRenderData.material = TextAtlas.material;

        using var uiFontRenderer = ObtainTextRenderer();
        if (uiFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer) {
            dynamicFontRenderer.spriteAtlas = TextAtlas;
            dynamicFontRenderer.spriteBuffer = _textRenderData;
        }

        uiFontRenderer.Render(_text, _textRenderData);
    }

    protected virtual void OnTextChanged() { }

    protected virtual Color32 GetTextColor() => InvalidateOnStateChanged ? TextColors.GetValue(_state) : isEnabled ? TextColors.Normal : TextColors.Disabled;
    protected virtual Color32 GetGradientBottomColor() => isEnabled ? GradientBottomNormalColor : GradientBottomDisabledColor;

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(32, 18);
        TextColors = new ColorStateValue(this);
    }
}