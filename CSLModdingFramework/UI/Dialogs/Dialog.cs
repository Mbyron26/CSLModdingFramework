using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Logging;
using CSLModdingFramework.UI.Controls;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Dialogs;

public abstract class Dialog : UIControl, IAutoLayout, IThemeable {
    protected int _autoLayoutSuspendCount;
    protected LiteBackgroundRenderer _background;
    protected float _buttonHeight = 40f;
    protected int _buttonSpacing = 40;
    protected List<Button> _buttons;
    protected float _buttonsYOffset;
    protected LiteContainer _contentContainer;
    protected DragArea _dragArea;
    protected int _fixedButtonWidth = 200;
    protected DynamicFont _font;
    protected LiteForegroundRenderer _foreground;
    protected float _padding = 30;
    protected ScrollViewer _scrollViewer;
    protected UIRenderData _textRenderData;
    protected ThemeKind _theme;
    protected string _title;
    protected Vector3 _titleLayoutOffset;
    protected Vector2 _titleRenderMaxSize;
    protected Vector2 _titleRenderSize;
    protected float _titleTextScale = 1.125f;
    private IReadOnlyList<Button> _readonlyButtons;
    public bool IsPerformingAutoLayout { get; protected set; }

    public virtual int ButtonSpacing {
        get => _buttonSpacing;
        set {
            if (_buttonSpacing == value) return;

            _buttonSpacing = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Vector2 DefaultSize => new(700, 200);

    public virtual int FixedButtonWidth {
        get => _fixedButtonWidth;
        set {
            if (_fixedButtonWidth == value) return;

            _fixedButtonWidth = value;
            InvalidateAutoLayout();
        }
    }

    public virtual ScrollViewer.ScrollSizePolicy ScrollContainerSizeMode => ScrollViewer.ScrollSizePolicy.FitHeight;

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public virtual string Title {
        get => _title;
        set {
            if (string.Equals(value, _title)) return;

            _title = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float TitleTextScale {
        get => _titleTextScale;
        set {
            if (Mathf.Approximately(value, _titleTextScale)) return;

            _titleTextScale = value;
            InvalidateAutoLayout();
        }
    }

    protected virtual float TitleHeight => 70;
    public LiteBackgroundRenderer Background => _background ??= new LiteBackgroundRenderer(this);

    public IReadOnlyList<Button> Buttons => _readonlyButtons ??= _buttons.AsReadOnlyList();
    public DynamicFont Font => _font ??= FontManager.OpenSansRegularFont;
    public LiteForegroundRenderer Foreground => _foreground ??= new LiteForegroundRenderer(this);
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        if (!string.IsNullOrEmpty(_title)) {
            _titleRenderMaxSize = new Vector2(width - 2 * _padding, TitleHeight);
            using var textRenderer = RentTitleRenderer();
            _titleRenderSize = textRenderer.GetRenderedSize(_title);
            _titleLayoutOffset = new Vector3(_padding, (TitleHeight - _titleRenderSize.y) / 2f);
        }
        else {
            _titleRenderMaxSize = Vector2.zero;
            _titleRenderSize = Vector2.zero;
            _titleLayoutOffset = Vector3.zero;
        }

        _scrollViewer.relativePosition = new Vector3(_padding, TitleHeight);
        _buttonsYOffset = _scrollViewer.relativePosition.y + _scrollViewer.height + 20;

        ArrangeButtonAndSetHeight();
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

    public virtual void SetTheme(ThemeKind theme) {
        if (theme == _theme) return;

        _theme = theme;
        Background.Atlas = DefaultAtlas.Instance;
        Background.SpriteName = DefaultAtlas.CustomBackground;
        Background.NormalColor = UIColorPalette.ClassicDarkPalette.RootBackground;
        Foreground.Atlas = DefaultAtlas.Instance;
        Foreground.Sprite = DefaultAtlas.SmoothRectangleRadius12Outline2;
        Foreground.ScaleMode = SpriteScaleMode.Stretch;
        Foreground.NormalColor = UIColorPalette.ClassicDarkPalette.RootForeground;
        _scrollViewer.VerticalScrollBar.ThumbControl.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingsSectionBackground);
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    protected virtual void ArrangeButtonAndSetHeight() {
        if (_buttons.Count == 0) {
            SetHeightIfChanged(_buttonsYOffset);
        }
        else {
            var count = _buttons.Count;
            float buttonWidth;
            float offsetX;

            if (_fixedButtonWidth != 0) {
                buttonWidth = _fixedButtonWidth;
                var totalButtonWidth = count * buttonWidth + (count - 1) * _buttonSpacing;
                offsetX = (width - totalButtonWidth) * 0.5f;
            }
            else {
                buttonWidth = (width - 2 * _padding - (count - 1) * _buttonSpacing) / count;
                offsetX = _padding;
            }

            for (var i = 0; i < count; i++) {
                _buttons[i].width = buttonWidth;
                _buttons[i].relativePosition = new Vector3(offsetX, _buttonsYOffset);
                offsetX += buttonWidth + _buttonSpacing;
            }

            SetHeightIfChanged(_buttons[0].relativePosition.y + _buttons[0].height + _padding);
        }
    }

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    protected virtual void OnAwake() {
        m_CanFocus = true;
        m_IsInteractive = true;
        m_Size = DefaultSize;
        _buttons = new List<Button>();

        _dragArea = AddUIComponent<DragArea>();
        _dragArea.size = new Vector2(width, TitleHeight);
        _dragArea.relativePosition = Vector3.zero;

        _scrollViewer = AddUIComponent<ScrollViewer>();
        _scrollViewer.SetWidthIfChanged(width - 2 * _padding + 8 + 4);
        _scrollViewer.SizeMode = ScrollContainerSizeMode;
        _scrollViewer.Orientation = ScrollOrientation.Vertical;
        _contentContainer = _scrollViewer.SetContent<LiteContainer>();
        _contentContainer.SuspendAutoLayout();
        _contentContainer.width = width - 2 * _padding;
        _contentContainer.AutoLayout = true;
        _contentContainer.AutoFitChildrenVertically = true;
        _contentContainer.ResumeAutoLayout();

        _scrollViewer.eventSizeChanged += (_, _) => InvalidateAutoLayout();
        SetTheme(ThemeKind.ClassicDark);
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

        if (IsAutoLayoutSuspended || string.IsNullOrEmpty(_title)) return;

        using var fontRenderer = RentTitleRenderer();
        fontRenderer.Render(_title, _textRenderData);
    }

    protected virtual DynamicFontRenderer RentTitleRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = false;
        fontRenderer.MaxSize = _titleRenderMaxSize;
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _titleTextScale;
        fontRenderer.LayoutOffset = _titleLayoutOffset;
        fontRenderer.TextAlign = UIHorizontalAlignment.Center;
        fontRenderer.DefaultColor = UIColorPalette.White;
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    public override void Awake() {
        base.Awake();

        OnAwake();
    }

    public override void OnDisable() {
        base.OnDisable();

        UnityEngine.Font.textureRebuilt -= OnUnityFontTextureRebuilt;
    }

    public override void OnEnable() {
        base.OnEnable();

        UnityEngine.Font.textureRebuilt += OnUnityFontTextureRebuilt;
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
        RenderText();
    }

    public static void Close(Dialog dialog, bool destroyImmediate = false) {
        if (dialog == null) return;

        var top = UIView.GetModalComponent();
        if (top != dialog) return;

        UIView.PopModal();

        var modalEffect = UIView.GetAView().panelsLibraryModalEffect;
        if (modalEffect != null) {
            if (!UIView.HasModalInput())
                ValueAnimator.Animate("ModalEffect67419", val => modalEffect.opacity = val, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), modalEffect.Hide);
            else
                modalEffect.zOrder = UIView.GetModalComponent().zOrder - 1;
        }

        DestroyDialog(dialog, destroyImmediate);
    }

    public static T Show<T>(Action<T> initializer = null) where T : Dialog {
        T dialog = null;
        try {
            var gameObject = new GameObject {
                transform = {
                    parent = UIView.GetAView().transform
                }
            };
            dialog = gameObject.AddComponent<T>();
            initializer?.Invoke(dialog);
            dialog.Show(true);
            dialog.Focus();
        }
        catch (Exception ex) {
            if (dialog != null)
                Destroy(dialog.gameObject);
            LogManager.GetLogger().Error(ex, $"Error showing dialog of type {typeof(T)}");
            return null;
        }

        var modalEffect = UIView.GetAView().panelsLibraryModalEffect;
        if (modalEffect != null) {
            modalEffect.FitTo(null);
            if (!UIView.HasModalInput()) {
                modalEffect.Show(false);
                ValueAnimator.Animate("ModalEffect67419", val => modalEffect.opacity = val, new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
            }

            modalEffect.zOrder = dialog.zOrder - 1;
        }

        UIView.PushModal(dialog);
        return dialog;
    }

    private static void DestroyDialog(Dialog dialog, bool destroyImmediate) {
        dialog.Hide();
        if (destroyImmediate)
            DestroyImmediate(dialog.gameObject);
        else
            Destroy(dialog.gameObject);
    }

    public Button AddButton(string text) {
        var button = AddUIComponent<Button>();
        button.size = new Vector2(100, _buttonHeight);
        button.ApplySettingsSectionStyle(_theme);
        _buttons.Add(button);
        button.Text = text;
        return button;
    }

    public void Close() => Close(this);
    private void OnUnityFontTextureRebuilt(Font font) => Invalidate();
}