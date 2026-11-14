using ColossalFramework.UI;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class SettingItem : SettingsControl, IThemeable {
    protected IAtlas _backgroundAtlas;
    protected Color32 _backgroundDisabledColor = UIColorPalette.White;
    protected float _backgroundHeight = 20f;
    protected EdgeInsetsHorizontal _backgroundHorizontalPadding = new(16f);
    protected Color32 _backgroundNormalColor = UIColorPalette.White;
    protected string _backgroundSpriteName = DefaultAtlas.LineBottom;
    protected UIComponent _content;
    protected Orientation _direction;
    protected EdgeInsets _layoutPadding;
    protected float _textContentGap = 10f;
    protected float _textGap = 6f;
    protected ThemeKind _theme;
    private float _descriptionHeight;
    private float _descriptionWidth;
    private float _descriptionYOffset;
    private float _headerHeight;
    private float _headerWidth;
    private float _headerYOffset;

    public IAtlas BackgroundAtlas {
        get => _backgroundAtlas;
        set {
            if (_backgroundAtlas == value) return;

            _backgroundAtlas = value;
            Invalidate();
        }
    }

    public Color32 BackgroundDisabledColor {
        get => _backgroundDisabledColor;
        set {
            if (_backgroundDisabledColor.EqualsColor(value)) return;

            _backgroundDisabledColor = value;
            Invalidate();
        }
    }

    public float BackgroundHeight {
        get => _backgroundHeight;
        set {
            if (Mathf.Approximately(value, _backgroundHeight)) return;

            _backgroundHeight = value;
            Invalidate();
        }
    }

    public EdgeInsetsHorizontal BackgroundHorizontalPadding {
        get => _backgroundHorizontalPadding;
        set {
            if (value.Equals(_backgroundHorizontalPadding)) return;

            _backgroundHorizontalPadding = value;
            InvalidateAutoLayout();
        }
    }

    public bool BackgroundIsEnabled { get; set; } = true;

    public Color32 BackgroundNormalColor {
        get => _backgroundNormalColor;
        set {
            if (_backgroundNormalColor.EqualsColor(value)) return;

            _backgroundNormalColor = value;
            Invalidate();
        }
    }

    public string BackgroundSpriteName {
        get => _backgroundSpriteName;
        set {
            if (_backgroundSpriteName.Equals(value)) return;

            _backgroundSpriteName = value;
            Invalidate();
        }
    }

    public UIComponent Content => _content;

    public Orientation Direction {
        get => _direction;
        set {
            if (value.Equals(_direction)) return;

            _direction = value;
            InvalidateAutoLayout();
        }
    }

    public int LayoutCount { get; private set; }

    public EdgeInsets LayoutPadding {
        get => _layoutPadding;
        set {
            if (_layoutPadding.Equals(value)) return;

            _layoutPadding = value;
            InvalidateAutoLayout();
        }
    }

    public float TextContentGap {
        get => _textContentGap;
        set {
            if (Mathf.Approximately(value, _textContentGap)) return;

            _textContentGap = value;
            InvalidateAutoLayout();
        }
    }

    public float TextGap {
        get => _textGap;
        set {
            if (Mathf.Approximately(value, _textGap)) return;

            _textGap = value;
            InvalidateAutoLayout();
        }
    }

    public ThemeKind Theme {
        get => _theme;
        set {
            if (value.Equals(_theme)) return;

            _theme = value;
            SetTheme(value);
        }
    }

    public virtual DynamicFontRenderer RentDescriptionRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = true;
        fontRenderer.MaxSize = new Vector2(_descriptionWidth, 4096);
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _descriptionScale;
        fontRenderer.LayoutOffset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + new Vector3(_layoutPadding.Left + _textHorizontalPadding.Left, _descriptionYOffset);
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = true;
        fontRenderer.DefaultColor = isEnabled ? _descriptionNormalColor : _descriptionDisabledColor;
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    public virtual DynamicFontRenderer RentHeaderRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = true;
        fontRenderer.MaxSize = new Vector2(_headerWidth, 4096);
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _headerScale;
        fontRenderer.LayoutOffset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + new Vector3(_layoutPadding.Left + _textHorizontalPadding.Left, _headerYOffset);
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = true;
        fontRenderer.DefaultColor = isEnabled ? HeaderNormalColor : HeaderDisabledColor;
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    public virtual void SetTheme(ThemeKind theme) {
        BackgroundAtlas = DefaultAtlas.Instance;

        switch (theme) {
            case ThemeKind.ClassicLight:
                BackgroundNormalColor = UIColorPalette.ClassicLightPalette.SettingItemBackground;
                BackgroundDisabledColor = UIColorPalette.ClassicLightPalette.SettingItemBackground;
                break;
            case ThemeKind.ClassicDark:
                BackgroundNormalColor = UIColorPalette.ClassicDarkPalette.SettingItemBackground;
                BackgroundDisabledColor = UIColorPalette.ClassicDarkPalette.SettingItemBackground;
                break;
            case ThemeKind.ModernDark:
            default:
                BackgroundNormalColor = UIColorPalette.ModernDarkPalette.SettingItemBackground;
                BackgroundDisabledColor = UIColorPalette.ModernDarkPalette.SettingItemBackground;
                break;
        }
    }

    protected virtual void RenderBackground() {
        if (_backgroundAtlas == null || !BackgroundIsEnabled) return;

        renderData.material = _backgroundAtlas.Material;

        if (!_backgroundAtlas.TryGetSprite(_backgroundSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _backgroundAtlas.GetSprite(spriteIndex);

        var renderOffset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
        renderOffset.y += size.y - _backgroundHeight;
        renderOffset.x += _backgroundHorizontalPadding.Left;

        SpriteRenderData spriteRenderData = new() {
            Atlas = _backgroundAtlas,
            Color = isEnabled ? _backgroundNormalColor : _backgroundDisabledColor,
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = renderOffset,
            UnitsPerPixel = UnitsPerPixel,
            Size = new Vector2(width - _backgroundHorizontalPadding.Horizontal, _backgroundHeight),
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }

    protected virtual void RenderDescription() {
        if (Font is not { IsValid: true }) return;

        if (_descriptionRenderData != null) {
            _descriptionRenderData.Clear();
        }
        else {
            _descriptionRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_descriptionRenderData);
        }

        if (string.IsNullOrEmpty(_header) || string.IsNullOrEmpty(_description)) return;

        using var fontRenderer = RentDescriptionRenderer();
        fontRenderer.Render(_description, _descriptionRenderData);
    }

    protected virtual void RenderHeader() {
        if (Font is not { IsValid: true }) return;

        if (_headerRenderData != null) {
            _headerRenderData.Clear();
        }
        else {
            _headerRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_headerRenderData);
        }

        if (string.IsNullOrEmpty(_header)) return;

        using var fontRenderer = RentHeaderRenderer();
        fontRenderer.Render(_header, _headerRenderData);
    }

    public override void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;
        if (_direction == Orientation.Horizontal) {
            HorizontalLayout();
        }
        else {
            VerticalLayout();
        }

        IsPerformingAutoLayout = false;
    }

    protected override Plane[] GetClippingPlanes() {
        if (!clipChildren) return null;

        var corners = GetCorners();
        var vector = transform.TransformDirection(Vector3.right);
        var vector2 = transform.TransformDirection(Vector3.left);
        var vector3 = transform.TransformDirection(Vector3.up);
        var vector4 = transform.TransformDirection(Vector3.down);
        var d = PixelsToUnits();
        var padding = _layoutPadding;
        corners[0] += vector * padding.Left * d + vector4 * padding.Top * d;
        corners[1] += vector2 * padding.Right * d + vector4 * padding.Top * d;
        corners[2] += vector2 * padding.Right * d + vector3 * padding.Bottom * d;
        m_CachedClippingPlanes[0] = new Plane(vector, corners[0]);
        m_CachedClippingPlanes[1] = new Plane(vector2, corners[1]);
        m_CachedClippingPlanes[2] = new Plane(vector3, corners[2]);
        m_CachedClippingPlanes[3] = new Plane(vector4, corners[0]);
        return m_CachedClippingPlanes;
    }

    protected override void OnRebuildRenderData() {
        if (IsAutoLayoutSuspended) return;

        RenderHeader();
        RenderDescription();
        RenderBackground();
    }

    public T AddContent<T>() where T : UIComponent {
        if (_content != null) {
            RemoveContent();
        }

        SuspendAutoLayout();
        var content = AddUIComponent<T>();
        _content = content;
        AttachControlEvents(content);
        InvalidateAutoLayout();
        ResumeAutoLayout();
        return content;
    }

    public T GetContent<T>() where T : UIComponent => _content as T;

    public void RemoveContent() {
        if (_content == null) return;

        SuspendAutoLayout();
        DetachControlEvents(_content);
        _content.DestroyChildrenAndSelf();
        ResumeAutoLayout();
    }

    private void AttachControlEvents(UIComponent control) {
        control.eventVisibilityChanged += OnChildVisibleChanged;
        control.eventSizeChanged += OnChildSizeChanged;
    }

    private void ChildInvalidatedLayout() => InvalidateAutoLayout();

    private void DetachControlEvents(UIComponent control) {
        control.eventVisibilityChanged -= OnChildVisibleChanged;
        control.eventSizeChanged -= OnChildSizeChanged;
    }

    private void HorizontalLayout() {
        LayoutCount++;

        var contentWidth = _content?.width ?? 0f;
        var contentHeight = _content?.height ?? 0f;

        var textWidth = width - _layoutPadding.Horizontal - _textHorizontalPadding.Horizontal;
        if (_content != null)
            textWidth -= contentWidth + _textContentGap;

        _headerWidth = 0f;
        _headerHeight = 0f;

        var headerIsNullOrEmpty = string.IsNullOrEmpty(_header);
        var descriptionIsNullOrEmpty = string.IsNullOrEmpty(_description);

        if (!headerIsNullOrEmpty) {
            _headerWidth = textWidth;

            using var headerRenderer = RentHeaderRenderer();
            _headerHeight = headerRenderer.GetRenderedSize(_header).y;
        }

        _descriptionWidth = 0f;
        _descriptionHeight = 0f;
        if (!descriptionIsNullOrEmpty) {
            _descriptionWidth = textWidth;
            using var descriptionRenderer = RentDescriptionRenderer();
            _descriptionHeight = descriptionRenderer.GetRenderedSize(_description).y;
        }

        var textBlockHeight = 0f;
        if (!headerIsNullOrEmpty) {
            textBlockHeight = _headerHeight;

            if (!descriptionIsNullOrEmpty)
                textBlockHeight += _textGap + _descriptionHeight;
        }

        var finalHeight = Mathf.Max(textBlockHeight, contentHeight) + _layoutPadding.Vertical;
        SetHeightIfChanged(finalHeight);

        float textStartY;
        float contentStartY;

        if (textBlockHeight >= contentHeight) {
            textStartY = _layoutPadding.Top;
            contentStartY = _layoutPadding.Top + (textBlockHeight - contentHeight) / 2f;
        }
        else {
            contentStartY = _layoutPadding.Top;
            textStartY = _layoutPadding.Top + (contentHeight - textBlockHeight) / 2f;
        }

        _headerYOffset = textStartY;

        if (!descriptionIsNullOrEmpty)
            _descriptionYOffset = _headerYOffset + _headerHeight + _textGap;

        if (_content != null) {
            var contentX = width - _layoutPadding.Right - contentWidth;
            _content.relativePosition = new Vector3(contentX, contentStartY, 0);
        }
    }

    private void OnChildSizeChanged(UIComponent child, Vector2 value) => ChildInvalidatedLayout();

    private void OnChildVisibleChanged(UIComponent child, bool value) => ChildInvalidatedLayout();

    private void VerticalLayout() {
        LayoutCount++;

        var textWidth = width - _layoutPadding.Horizontal - _textHorizontalPadding.Horizontal;

        _headerWidth = 0f;
        _headerHeight = 0f;
        var headerIsNullOrEmpty = string.IsNullOrEmpty(_header);
        var descriptionIsNullOrEmpty = string.IsNullOrEmpty(_description);
        if (!headerIsNullOrEmpty) {
            _headerWidth = textWidth;
            using (var headerRenderer = RentHeaderRenderer()) {
                _headerHeight = headerRenderer.GetRenderedSize(_header).y;
            }
        }

        _descriptionWidth = 0f;
        _descriptionHeight = 0f;

        if (!headerIsNullOrEmpty && !descriptionIsNullOrEmpty) {
            _descriptionWidth = textWidth;
            using (var descriptionRenderer = RentDescriptionRenderer()) {
                _descriptionHeight = descriptionRenderer.GetRenderedSize(_description).y;
            }
        }

        var textBlockHeight = 0f;
        if (!headerIsNullOrEmpty) {
            textBlockHeight = _headerHeight;

            if (!descriptionIsNullOrEmpty)
                textBlockHeight += _textGap + _descriptionHeight;
        }

        _headerYOffset = _layoutPadding.Top;

        if (!descriptionIsNullOrEmpty)
            _descriptionYOffset = _headerYOffset + _headerHeight + _textGap;

        var contentY = _layoutPadding.Top + textBlockHeight;

        if (_content != null) {
            contentY += textBlockHeight > 0 ? _textContentGap : 0;

            _content.relativePosition = new Vector3(_layoutPadding.Left, contentY);

            contentY += _content.height;
        }

        var finalHeight = contentY + _layoutPadding.Bottom;

        SetHeightIfChanged(finalHeight);
    }
}