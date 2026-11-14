using ColossalFramework.UI;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class TextControl : UIInteractiveControl, IAutoLayout {
    protected UIRenderData _textRenderData;
    protected DynamicFont _font = FontManager.OpenSansRegularFont;
    protected string _text = string.Empty;
    protected float _textScale = 1f;
    protected EdgeInsets _textPadding;
    protected float _textCharacterSpacing;
    protected bool _textProcessMarkup;
    protected UIHorizontalAlignment _textHorizontalAlignment = UIHorizontalAlignment.Center;
    protected UIVerticalAlignment _textVerticalAlignment = UIVerticalAlignment.Middle;
    protected float _textLineSpacing;
    protected float _textSpaceAdditionalWidth = 1;
    protected bool _textWordWrap;
    protected int _textTabSize = 48;
    protected TextSizeMode _sizeMode;
    protected Vector2 _textRenderPosition;
    protected float _textRenderMaxWidth;
    protected float _textRenderMaxHeight;
    protected Size _minSize;
    protected Size _maxSize;
    protected int _autoLayoutSuspendCount;

    public bool IsPerformingAutoLayout { get; protected set; }
    public ColorStateValue TextColors { get; private set; }

    public DynamicFont Font {
        get => _font;
        set {
            if (ReferenceEquals(value, _font)) return;

            _font = value;
            Invalidate();
        }
    }

    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public virtual string Text {
        get => _text;
        set => SetText(value);
    }

    public virtual float TextSpaceAdditionalWidth {
        get => _textSpaceAdditionalWidth;
        set {
            if (Mathf.Approximately(_textSpaceAdditionalWidth, value)) return;

            _textSpaceAdditionalWidth = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float TextLineSpacing {
        get => _textLineSpacing;
        set {
            if (Mathf.Approximately(_textLineSpacing, value)) return;

            _textLineSpacing = value;
            InvalidateAutoLayout();
        }
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

    public virtual float TextCharacterSpacing {
        get => _textCharacterSpacing;
        set {
            value = Mathf.Max(0, value);
            if (Mathf.Approximately(value, _textCharacterSpacing)) return;

            _textCharacterSpacing = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool TextProcessMarkup {
        get => _textProcessMarkup;
        set {
            if (value == _textProcessMarkup) return;

            _textProcessMarkup = value;
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

    public virtual bool TextWordWrap {
        get => _textWordWrap;
        set {
            if (value == _textWordWrap) return;

            _textWordWrap = value;
            InvalidateAutoLayout();
        }
    }

    public virtual int TextTabSize {
        get => _textTabSize;
        set {
            value = Mathf.Max(0, value);
            if (value == _textTabSize) return;

            _textTabSize = value;
            InvalidateAutoLayout();
        }
    }

    public virtual DynamicFontRenderer RentTextRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.CharacterSpacing = _textCharacterSpacing;
        fontRenderer.WordWrap = _textWordWrap;
        fontRenderer.LineSpacing = _textLineSpacing;
        fontRenderer.SpaceAdditionalWidth = _textSpaceAdditionalWidth;
        fontRenderer.MaxSize = new Vector2(_textRenderMaxWidth, _textRenderMaxHeight);
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _textScale;
        fontRenderer.LayoutOffset = _textRenderPosition;
        fontRenderer.TextAlign = _textHorizontalAlignment;
        fontRenderer.ProcessMarkup = _textProcessMarkup;
        fontRenderer.DefaultColor = GetTextColor();
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    public virtual bool SetTextWithoutNotify(string text) => SetText(text, false);

    public virtual bool SetText(string text, bool notify = true) {
        if (string.Equals(text, _text)) return false;

        _text = text;
        InvalidateAutoLayout();

        if (notify)
            OnTextChanged();

        return true;
    }

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;
        var textIntrinsicSize = MeasureRenderedTextSize();

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
                _textRenderMaxWidth = intrinsicWidth;
                finalTextOffsetX = _textPadding.Left;

                if (paddedHeight < minHeight) {
                    _textRenderMaxHeight = minHeight - _textPadding.Vertical;
                    finalHeight = minHeight;
                    finalTextOffsetY = _textPadding.Top;
                }
                else if (paddedHeight > maxHeight) {
                    _textRenderMaxHeight = maxHeight - _textPadding.Vertical;
                    finalHeight = maxHeight;

                    finalTextOffsetY = _textVerticalAlignment switch {
                        UIVerticalAlignment.Top => _textPadding.Top,
                        UIVerticalAlignment.Bottom => finalHeight - _textRenderMaxHeight - _textPadding.Bottom,
                        _ => (finalHeight - _textRenderMaxHeight) / 2
                    };
                }
                else {
                    _textRenderMaxHeight = intrinsicHeight;
                    finalHeight = _textRenderMaxHeight + _textPadding.Vertical;
                    finalTextOffsetY = _textPadding.Top;
                }

                finalWidth = minWidth;
            }
            else if (paddedWidth >= maxWidth) {
                _textRenderMaxWidth = maxWidth - _textPadding.Horizontal;
                var newTextHeight = MeasureRenderedTextSize(_textRenderMaxWidth, allowWordWrap: _textWordWrap).y;

                finalTextOffsetX = _textPadding.Left;

                if (newTextHeight <= minHeight) {
                    _textRenderMaxHeight = minHeight - _textPadding.Vertical;
                    finalHeight = minHeight;
                    finalTextOffsetY = _textVerticalAlignment switch {
                        UIVerticalAlignment.Top => _textPadding.Top,
                        UIVerticalAlignment.Bottom => finalHeight - _textRenderMaxHeight - _textPadding.Bottom,
                        _ => (finalHeight - _textRenderMaxHeight) / 2
                    };
                }
                else if (newTextHeight >= maxHeight) {
                    _textRenderMaxHeight = maxHeight - _textPadding.Vertical;
                    finalHeight = maxHeight;
                    finalTextOffsetY = _textPadding.Top;
                }
                else {
                    _textRenderMaxHeight = newTextHeight;
                    finalHeight = _textRenderMaxHeight + _textPadding.Vertical;
                    finalTextOffsetY = _textPadding.Top;
                }

                finalWidth = maxWidth;
            }
            else {
                _textRenderMaxWidth = intrinsicWidth;
                _textRenderMaxHeight = intrinsicHeight;
                finalWidth = _textRenderMaxWidth + _textPadding.Horizontal;
                finalHeight = _textRenderMaxHeight + _textPadding.Vertical;
                finalTextOffsetX = _textPadding.Left;
                finalTextOffsetY = _textPadding.Top;
            }
        }
        else if (_sizeMode == TextSizeMode.FitWidth) {
            _textHorizontalAlignment = UIHorizontalAlignment.Left;
            if (_maxSize.IsNearlyZero())
                _maxSize = new Size(4096, 4096);

            var intrinsicWidth = textIntrinsicSize.x;
            var intrinsicHeight = textIntrinsicSize.y;

            var paddedWidth = intrinsicWidth + _textPadding.Horizontal;

            var minWidth = _minSize.Width;
            var maxWidth = _maxSize.Width;

            _textRenderMaxHeight = m_Size.y - _textPadding.Vertical;
            finalHeight = m_Size.y;
            finalTextOffsetX = _textPadding.Left;

            if (paddedWidth <= minWidth) {
                _textRenderMaxWidth = minWidth - _textPadding.Horizontal;
                finalWidth = minWidth;

                finalTextOffsetY = _textVerticalAlignment switch {
                    UIVerticalAlignment.Top => _textPadding.Top,
                    UIVerticalAlignment.Bottom => finalHeight - intrinsicHeight - _textPadding.Bottom,
                    _ => (finalHeight - intrinsicHeight) / 2
                };
            }
            else if (paddedWidth > maxWidth) {
                _textRenderMaxWidth = maxWidth - _textPadding.Horizontal;
                var newTextHeight = MeasureRenderedTextSize(_textRenderMaxWidth, allowWordWrap: _textWordWrap).y;
                finalWidth = maxWidth;

                finalTextOffsetY = _textVerticalAlignment switch {
                    UIVerticalAlignment.Top => _textPadding.Top,
                    UIVerticalAlignment.Bottom => finalHeight - newTextHeight - _textPadding.Bottom,
                    _ => (finalHeight - newTextHeight) / 2
                };
            }
            else {
                _textRenderMaxWidth = paddedWidth;
                finalWidth = paddedWidth;

                finalTextOffsetY = _textVerticalAlignment switch {
                    UIVerticalAlignment.Top => _textPadding.Top,
                    UIVerticalAlignment.Bottom => finalHeight - intrinsicHeight - _textPadding.Bottom,
                    _ => (finalHeight - intrinsicHeight) / 2
                };
            }
        }
        else if (_sizeMode == TextSizeMode.FitHeight) {
            finalWidth = m_Size.x;
            var textSize = MeasureRenderedTextSize(finalWidth - _textPadding.Horizontal, 4096, _textWordWrap);

            _textRenderMaxWidth = Mathf.Clamp(textSize.x, 0, finalWidth);
            _textRenderMaxHeight = Mathf.Clamp(textSize.y, 0, 4096);
            finalHeight = _textRenderMaxHeight + _textPadding.Vertical;
            finalTextOffsetX = _textHorizontalAlignment switch {
                UIHorizontalAlignment.Left => _textPadding.Left,
                UIHorizontalAlignment.Right => finalWidth - _textRenderMaxWidth - _textPadding.Right,
                _ => (finalWidth - _textRenderMaxWidth) / 2
            };

            finalTextOffsetY = _textVerticalAlignment switch {
                UIVerticalAlignment.Top => _textPadding.Top,
                UIVerticalAlignment.Bottom => finalHeight - _textRenderMaxHeight - _textPadding.Bottom,
                _ => (finalHeight - _textRenderMaxHeight) / 2
            };
        }
        else {
            finalWidth = m_Size.x;
            finalHeight = m_Size.y;

            var textSize = MeasureRenderedTextSize(finalWidth - _textPadding.Horizontal, finalHeight > 4096 ? finalHeight : 4096, _textWordWrap);

            _textRenderMaxWidth = Mathf.Clamp(textSize.x, 0, finalWidth);
            _textRenderMaxHeight = Mathf.Clamp(textSize.y, 0, finalHeight);

            finalTextOffsetX = _textHorizontalAlignment switch {
                UIHorizontalAlignment.Left => _textPadding.Left,
                UIHorizontalAlignment.Right => finalWidth - _textRenderMaxWidth - _textPadding.Right,
                _ => (finalWidth - _textRenderMaxWidth) / 2
            };

            finalTextOffsetY = _textVerticalAlignment switch {
                UIVerticalAlignment.Top => _textPadding.Top,
                UIVerticalAlignment.Bottom => finalHeight - _textRenderMaxHeight - _textPadding.Bottom,
                _ => (finalHeight - _textRenderMaxHeight) / 2
            };
        }

        SetSizeInternal(new Vector2(finalWidth, finalHeight));
        _textRenderPosition = new Vector2(finalTextOffsetX, finalTextOffsetY);
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

    public virtual Vector2 MeasureRenderedTextSize(float maxWidth = 4096, float maxHeight = 4096, bool allowWordWrap = false) {
        if (string.IsNullOrEmpty(_text)) return Vector2.zero;

        using var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = allowWordWrap;
        fontRenderer.MaxSize = new Vector2(maxWidth, maxHeight);
        fontRenderer.PixelRatio = PixelsToUnits();
        fontRenderer.TextScale = _textScale;
        fontRenderer.LayoutOffset = Vector3.zero;
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = _textProcessMarkup;
        fontRenderer.DefaultColor = UIColorPalette.White;
        fontRenderer.SpaceAdditionalWidth = _textSpaceAdditionalWidth;
        fontRenderer.CharacterSpacing = _textCharacterSpacing;
        fontRenderer.LineSpacing = _textLineSpacing;
        return fontRenderer.GetRenderedSize(_text);
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

        if (string.IsNullOrEmpty(_text)) return;

        using var fontRenderer = RentTextRenderer();
        fontRenderer.Render(_text, _textRenderData);
    }

    protected virtual void OnTextChanged() { }

    protected virtual Color32 GetTextColor() => InvalidateOnStateChanged ? TextColors.GetValue(_state) : isEnabled ? TextColors.Normal : TextColors.Disabled;

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

    public override void OnEnable() {
        base.OnEnable();

        UnityEngine.Font.textureRebuilt += OnUnityFontTextureRebuilt;
    }

    public override void OnDisable() {
        base.OnDisable();

        UnityEngine.Font.textureRebuilt -= OnUnityFontTextureRebuilt;
    }

    private void OnUnityFontTextureRebuilt(Font font) => Invalidate();
}