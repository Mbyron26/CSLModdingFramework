using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class PopupTrigger<TPopup> : TextControl, IThemeable where TPopup : UIComponent {
    protected Vector2 _arrowIconOffset = new(-6f, 0);
    protected float _arrowIconScale = 1f;
    protected Vector2 _arrowIconSize = new(18, 18);
    protected string _arrowIconSpriteName;
    protected float _arrowIconTextGap;
    protected IAtlas _atlas;
    protected string _backgroundSpriteName;
    protected ThemeKind _theme;

    public event UIEventHandler<TextControl, TPopup> PopupCreated;

    public virtual Vector2 ArrowIconOffset {
        get => _arrowIconOffset;
        set {
            if (value.Equals(_arrowIconOffset)) return;

            _arrowIconOffset = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float ArrowIconScale {
        get => _arrowIconScale;
        set {
            if (Mathf.Approximately(value, _arrowIconScale)) return;

            _arrowIconScale = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Vector2 ArrowIconSize {
        get => _arrowIconSize;
        set {
            if (value.Equals(_arrowIconSize)) return;

            _arrowIconSize = value;
            InvalidateAutoLayout();
        }
    }

    public virtual string ArrowIconSpriteName {
        get => _arrowIconSpriteName;
        set {
            if (string.Equals(_arrowIconSpriteName, value)) return;

            _arrowIconSpriteName = value;
            Invalidate();
        }
    }

    public virtual float ArrowIconTextGap {
        get => _arrowIconTextGap;
        set {
            if (Mathf.Approximately(value, _arrowIconTextGap)) return;

            _arrowIconTextGap = value;
            InvalidateAutoLayout();
        }
    }

    public virtual IAtlas Atlas {
        get => _atlas;
        set {
            if (_atlas == value) return;

            _atlas = value;
            Invalidate();
        }
    }

    public virtual string BackgroundSpriteName {
        get => _backgroundSpriteName;
        set {
            if (string.Equals(_backgroundSpriteName, value)) return;

            _backgroundSpriteName = value;
            Invalidate();
        }
    }

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public override TextSizeMode SizeMode => _sizeMode;
    public override bool TextWordWrap => _textWordWrap;

    public Anchor ArrowIconAnchor => Anchor.MiddleRight;
    public ColorStateValue ArrowIconColors { get; private set; }
    public ColorStateValue BackgroundColors { get; private set; }
    public bool ClampPopupToScreen { get; set; } = true;
    public bool ClosePopupAfterSelected { get; set; } = true;

    public bool ClosePopupOnClickOutside { get; set; } = true;
    public PopupPosition ListPosition { get; set; } = PopupPosition.Below;
    public TPopup Popup { get; protected internal set; }

    protected abstract TPopup GetOrCreatePopup();

    public virtual void SetTheme(ThemeKind theme) {
        if (theme == _theme) return;

        _theme = theme;

        _backgroundSpriteName = DefaultAtlas.SmoothRectangleRadius6;
        BackgroundColors.SetValues(theme == ThemeKind.ClassicLight ? UIColorPalette.ClassicLightPalette.SettingItemControlColors : UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
        _arrowIconSpriteName = DefaultAtlas.DownArrow;
        ArrowIconColors.SetValues(UIColorPalette.HeaderTextColors);
        Atlas = DefaultAtlas.Instance;
    }

    protected virtual void RenderArrowIcon() {
        if (string.IsNullOrEmpty(_arrowIconSpriteName) || !_atlas.TryGetSprite(_arrowIconSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);
        var renderSize = _arrowIconSize * _arrowIconScale;
        var offset = ArrowIconAnchor.CalculateTopLeftAnchorOffset(size, renderSize, _arrowIconOffset);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = ArrowIconColors.GetValue(_state),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = offset,
            UnitsPerPixel = UnitsPerPixel,
            Size = renderSize,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }

    protected virtual void RenderBackground() {
        if (string.IsNullOrEmpty(_backgroundSpriteName) || !_atlas.TryGetSprite(_backgroundSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = BackgroundColors.GetValue(_state),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = Vector3.zero,
            UnitsPerPixel = UnitsPerPixel,
            Size = size,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }

    protected virtual void TryClosePopupOnClickOutside() {
        if (!ClosePopupOnClickOutside) return;

        if (Popup == null || !Input.GetMouseButtonDown(0)) return;

        if (!Popup.isVisible) return;

        var camera = GetCamera();
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Popup.Raycast(ray)) return;

        if (Raycast(ray)) return;

        Popup.isVisible = false;
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(200, 30);
        _textHorizontalAlignment = UIHorizontalAlignment.Left;
        BackgroundColors = new ColorStateValue(this);
        ArrowIconColors = new ColorStateValue(this);
        TextColors.SetValues(UIColorPalette.HeaderTextColors);
        TextColors.SetValues(UIColorPalette.HeaderTextColors);
    }

    public override void OnDestroy() {
        base.OnDestroy();

        if (Popup != null) {
            Popup.Hide();
            Popup.DestroyChildrenAndSelf();
        }
    }

    public override void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        var iconSize = _arrowIconSize * _arrowIconScale;
        var iconOffset = ArrowIconAnchor.CalculateTopLeftAnchorOffset(size, iconSize, _arrowIconOffset);
        var iconLeft = iconOffset.x;

        _textRenderMaxWidth = Mathf.Max(0, iconLeft - _arrowIconTextGap - _textPadding.Horizontal);
        _textRenderMaxHeight = Mathf.Max(0, size.y - _textPadding.Vertical);

        var textIntrinsicSize = MeasureRenderedTextSize(allowWordWrap: false);
        var shouldShowTooltip = _textRenderMaxWidth < textIntrinsicSize.x;

        var textRegionX = _textPadding.Left;

        float finalTextOffsetX;

        if (shouldShowTooltip) {
            finalTextOffsetX = textRegionX;
        }
        else {
            finalTextOffsetX = _textHorizontalAlignment switch {
                UIHorizontalAlignment.Left => textRegionX,
                UIHorizontalAlignment.Right => iconLeft - _arrowIconTextGap - textIntrinsicSize.x,
                _ => textRegionX + (_textRenderMaxWidth - textIntrinsicSize.x) / 2f
            };
        }

        var finalTextOffsetY = _textVerticalAlignment switch {
            UIVerticalAlignment.Top => _textPadding.Top,
            UIVerticalAlignment.Bottom => size.y - _textPadding.Bottom - textIntrinsicSize.y,
            _ => _textPadding.Top + (_textRenderMaxHeight - textIntrinsicSize.y) / 2f
        };

        _textRenderPosition = new Vector2(finalTextOffsetX, finalTextOffsetY);

        if (tooltipBox != null)
            tooltip = shouldShowTooltip ? _text : null;

        IsPerformingAutoLayout = false;
    }

    public override void Update() {
        base.Update();

        TryClosePopupOnClickOutside();
    }

    protected override void OnClick(UIMouseEventParameter p) {
        base.OnClick(p);

        if (p.used) return;

        p.Use();

        if (Popup != null && Popup.isVisible) {
            HidePopup();
        }
        else {
            OpenPopup();
        }
    }

    protected override void OnRebuildRenderData() {
        if (IsAutoLayoutSuspended) return;

        if (_atlas != null) {
            renderData.material = _atlas.Material;
            RenderBackground();
            RenderArrowIcon();
        }

        RenderText();
    }

    public void HidePopup() {
        var popup = GetOrCreatePopup();
        popup.isVisible = false;
        popup.Unfocus();
    }

    public void OpenPopup() {
        var popup = GetOrCreatePopup();

        popup.isVisible = true;
        popup.Focus();
        SetPopupPosition();
    }

    public void SetPopupPosition() {
        if (Popup == null) return;

        var popupParent = Popup.parent;
        if (popupParent == null) return;

        var dropdownAbs = absolutePosition;

        Vector3 desiredWorldPos;
        var uiView = GetUIView();

        switch (ListPosition) {
            case PopupPosition.Below:
                desiredWorldPos = new Vector3(dropdownAbs.x, dropdownAbs.y + height, 0f);
                break;
            case PopupPosition.Above:
                desiredWorldPos = new Vector3(dropdownAbs.x, dropdownAbs.y - Popup.height, 0f);
                break;
            case PopupPosition.Automatic:
                var dropdownBottomY = dropdownAbs.y + height;
                var placeAbove = dropdownBottomY + Popup.height > uiView.fixedHeight;
                desiredWorldPos = placeAbove
                    ? new Vector3(dropdownAbs.x, dropdownAbs.y - Popup.height, 0f)
                    : new Vector3(dropdownAbs.x, dropdownAbs.y + height, 0f);
                break;
            default:
                desiredWorldPos = new Vector3(dropdownAbs.x, dropdownAbs.y + height, 0f);
                break;
        }

        var parentAbs = popupParent.absolutePosition;
        var relative = new Vector3(
            desiredWorldPos.x - parentAbs.x,
            desiredWorldPos.y - parentAbs.y,
            desiredWorldPos.z - parentAbs.z
        );

        Popup.relativePosition = relative;

        if (ClampPopupToScreen) {
            var worldPos = Popup.absolutePosition;
            var clampedX = Mathf.Clamp(worldPos.x, 0f, uiView.fixedWidth - Popup.width);
            var clampedY = Mathf.Clamp(worldPos.y, 0f, uiView.fixedHeight - Popup.height);
            Popup.absolutePosition = new Vector3(clampedX, clampedY, worldPos.z);
        }
    }

    protected void NotifyPopupCreated(TextControl sender, TPopup popup) => PopupCreated?.Invoke(sender, popup);
}