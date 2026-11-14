using ColossalFramework.UI;
using CSLModdingFramework.UI.Fonts;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class SelectionControl : UIInteractiveControl, IAutoLayout {
    protected int _autoLayoutSuspendCount;
    protected string _description;
    protected Color32 _descriptionDisabledColor = UIColorPalette.DescriptionTextColors.Disabled;
    protected Color32 _descriptionNormalColor = UIColorPalette.DescriptionTextColors.Normal;
    protected float _descriptionScale = 0.8f;
    protected Vector2 _descriptionSize;
    protected DynamicFont _font = FontManager.OpenSansRegularFont;
    protected string _header;
    protected Color32 _headerDisabledColor = UIColorPalette.HeaderTextColors.Disabled;
    protected Color32 _headerNormalColor = UIColorPalette.HeaderTextColors.Normal;
    protected UIRenderData _textRenderData;
    protected float _headerScale = 1f;
    protected Vector2 _headerSize;
    protected Vector2 _indicatorSize;
    protected float _indicatorTextGap = 10f;
    protected EdgeInsets _layoutPadding;
    protected float _textGap = 4f;

    public virtual string Description {
        get => _description;
        set {
            if (value == null || value.Equals(_description)) return;

            _description = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Color32 DescriptionDisabledColor {
        get => _descriptionDisabledColor;
        set {
            if (_descriptionDisabledColor.EqualsColor(value)) return;

            _descriptionDisabledColor = value;
            Invalidate();
        }
    }

    public virtual Color32 DescriptionNormalColor {
        get => _descriptionNormalColor;
        set {
            if (_descriptionNormalColor.EqualsColor(value)) return;

            _descriptionNormalColor = value;
            Invalidate();
        }
    }

    public virtual float DescriptionScale {
        get => _descriptionScale;
        set {
            if (Mathf.Approximately(_descriptionScale, value)) return;

            _descriptionScale = value;
            InvalidateAutoLayout();
        }
    }

    public virtual DynamicFont Font {
        get => _font;
        set {
            if (ReferenceEquals(value, _font)) return;

            _font = value;
            InvalidateAutoLayout();
        }
    }

    public virtual string Header {
        get => _header;
        set {
            if (value == null || value.Equals(_header)) return;

            _header = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Color32 HeaderDisabledColor {
        get => _headerDisabledColor;
        set {
            if (_headerDisabledColor.EqualsColor(value)) return;

            _headerDisabledColor = value;
            Invalidate();
        }
    }

    public virtual Color32 HeaderNormalColor {
        get => _headerNormalColor;
        set {
            if (_headerNormalColor.EqualsColor(value)) return;

            _headerNormalColor = value;
            Invalidate();
        }
    }

    public virtual float HeaderScale {
        get => _headerScale;
        set {
            if (Mathf.Approximately(_headerScale, value)) return;

            _headerScale = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Vector2 IndicatorSize {
        get => _indicatorSize;
        set {
            if (value.Equals(_indicatorSize)) return;

            _indicatorSize = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float IndicatorTextGap {
        get => _indicatorTextGap;
        set {
            if (Mathf.Approximately(_indicatorTextGap, value)) return;

            _indicatorTextGap = value;
            InvalidateAutoLayout();
        }
    }

    public virtual EdgeInsets LayoutPadding {
        get => _layoutPadding;
        set {
            if (_layoutPadding.Equals(value)) return;

            _layoutPadding = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float TextGap {
        get => _textGap;
        set {
            if (Mathf.Approximately(_textGap, value)) return;

            _textGap = value;
            InvalidateAutoLayout();
        }
    }

    public Vector2 DescriptionRenderSize { get; private set; }
    public Vector2 HeaderRenderSize { get; private set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;
    public bool IsPerformingAutoLayout { get; protected set; }

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        var textWidth = width
                        - _layoutPadding.Horizontal
                        - _indicatorSize.x
                        - _indicatorTextGap;

        _headerSize = new Vector2(textWidth, 0);
        _descriptionSize = new Vector2(textWidth, 0);

        var textHeight = 0f;

        if (!string.IsNullOrEmpty(_header)) {
            using var headerRenderer = RentHeaderRenderer();

            var headerSize = headerRenderer.GetRenderedSize(_header);
            _headerSize = new Vector2(textWidth, headerSize.y);

            textHeight = headerSize.y;

            if (!string.IsNullOrEmpty(_description)) {
                using var descriptionRenderer = RentDescriptionRenderer();

                var descriptionSize = descriptionRenderer.GetRenderedSize(_description);
                _descriptionSize = new Vector2(textWidth, descriptionSize.y);

                textHeight += _textGap + descriptionSize.y;
            }
        }

        var contentHeight = Mathf.Max(_indicatorSize.y, textHeight);
        var newHeight = _layoutPadding.Vertical + contentHeight;

        SetHeightIfChanged(newHeight);

        IsPerformingAutoLayout = false;
    }

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount == 0 && performNow) {
            PerformAutoLayout();
            Invalidate();
        }
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    protected virtual DynamicFontRenderer RentDescriptionRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = true;
        fontRenderer.MaxSize = new Vector2(_descriptionSize.x, 4096);
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _descriptionScale;
        fontRenderer.LayoutOffset = new Vector3(_layoutPadding.Left + _indicatorSize.x + _indicatorTextGap, _layoutPadding.Top + _headerSize.y + _textGap);
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = true;
        fontRenderer.DefaultColor = isEnabled ? _descriptionNormalColor : _descriptionDisabledColor;
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    protected virtual DynamicFontRenderer RentHeaderRenderer() {
        var pixelsToUnits = UnitsPerPixel;
        var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = true;
        fontRenderer.MaxSize = new Vector2(_headerSize.x, 4096);
        fontRenderer.PixelRatio = pixelsToUnits;
        fontRenderer.TextScale = _headerScale;
        fontRenderer.LayoutOffset = new Vector3(_layoutPadding.Left + _indicatorSize.x + _indicatorTextGap, _layoutPadding.Top);
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = true;
        fontRenderer.DefaultColor = isEnabled ? HeaderNormalColor : HeaderDisabledColor;
        fontRenderer.Opacity = 1f;
        return fontRenderer;
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

        if (string.IsNullOrEmpty(_header)) return;

        using (var fontRenderer = RentHeaderRenderer()) {
            fontRenderer.Render(_header, _textRenderData);
            HeaderRenderSize = fontRenderer.RenderedSize;
        }

        if (!string.IsNullOrEmpty(_description)) {
            using (var fontRenderer = RentDescriptionRenderer()) {
                fontRenderer.Render(_description, _textRenderData);
                DescriptionRenderSize = fontRenderer.RenderedSize;
            }
        }
    }

    public override void OnDisable() {
        base.OnDisable();

        UnityEngine.Font.textureRebuilt -= OnUnityFontTextureRebuilt;
    }

    public override void OnEnable() {
        base.OnEnable();

        UnityEngine.Font.textureRebuilt += OnUnityFontTextureRebuilt;
    }

    private void OnUnityFontTextureRebuilt(Font font) => Invalidate();
}