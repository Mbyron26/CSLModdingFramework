using System;
using ColossalFramework.UI;
using CSLModdingFramework.Core;
using CSLModdingFramework.Localization;
using CSLModdingFramework.UI.Controls;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Dialogs;

public class ExceptionDialog : Dialog {
    protected string _message;
    protected string _details;
    protected ExceptionDialogSeverity _severity;
    protected SpriteView _severityImage;
    protected TextField _detailsTextField;
    protected Vector2 _messageRenderSize;
    protected Vector3 _messageLayoutOffset;

    public override Vector2 DefaultSize => new(700, 200);

    public virtual string Message {
        get => _message;
        set {
            if (string.Equals(value, _message)) return;

            _message = value;
            InvalidateAutoLayout();
        }
    }

    public virtual string Details {
        get => _details;
        set {
            if (string.Equals(value, _details)) return;

            _details = value;
            InitDetailsTextField(_details);
            InvalidateAutoLayout();
        }
    }

    public virtual ExceptionDialogSeverity Severity {
        get => _severity;
        set {
            if (value == _severity) return;

            _severity = value;
            switch (_severity) {
                case ExceptionDialogSeverity.Error:
                    _severityImage.SpriteName = DefaultAtlas.Error;
                    break;
                case ExceptionDialogSeverity.Warning:
                    _severityImage.SpriteName = DefaultAtlas.Warning;
                    break;
            }

            Invalidate();
        }
    }

    protected virtual DynamicFontRenderer RentMessageRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = true;
        fontRenderer.MaxSize = _titleRenderMaxSize;
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = 1f;
        fontRenderer.LayoutOffset = _messageLayoutOffset;
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.DefaultColor = _severity == ExceptionDialogSeverity.Error ? UIColorPalette.RedNormal : UIColorPalette.YellowNormal;
        fontRenderer.ProcessMarkup = false;
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    protected override void RenderText() {
        if (Font is not { IsValid: true }) return;

        if (_textRenderData != null) {
            _textRenderData.Clear();
        }
        else {
            _textRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_textRenderData);
        }

        if (IsAutoLayoutSuspended) return;

        if (!string.IsNullOrEmpty(_title)) {
            using var fontRenderer = RentTitleRenderer();
            fontRenderer.Render(_title, _textRenderData);
        }

        if (!string.IsNullOrEmpty(_message)) {
            using var fontRenderer = RentMessageRenderer();
            fontRenderer.Render(_message, _textRenderData);
        }
    }

    protected override void OnAwake() {
        base.OnAwake();

        _severityImage = AddUIComponent<SpriteView>();
        _severityImage.Atlas = DefaultAtlas.Instance;
        _severityImage.size = new Vector2(64, 64);
        _severityImage.SpriteName = DefaultAtlas.Error;

        _scrollViewer.MaxFitSize = new Size(width, 400);

        var okButton = AddButton("OK");
        okButton.Clicked += () => Close(this);
        okButton.Background.Colors.SetValues(UIColorPalette.BlueColors);
        var copyButton = AddButton("Copy");
        copyButton.Clicked += OnCopy;
        copyButton.Background.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingsSectionColors);
        InvalidateAutoLayout();
        CenterToParent();
    }

    public override void SetTheme(ThemeKind theme) {
        base.SetTheme(theme);
        _scrollViewer.Background.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingsSectionBackground);
        _scrollViewer.Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
        _scrollViewer.Background.Atlas = DefaultAtlas.Instance;
        _scrollViewer.Background.Padding = new EdgeInsets(0, 10, 0, 0);
        _scrollViewer.Foreground.Colors.SetValues(UIColorPalette.ModernDarkPalette.SettingsSectionForeground);
        _scrollViewer.Foreground.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius6Outline1);
        _scrollViewer.Foreground.Atlas = DefaultAtlas.Instance;
        _scrollViewer.Foreground.Padding = new EdgeInsets(0, 10, 0, 0);
        _scrollViewer.Foreground.ScaleMode = SpriteScaleMode.Stretch;
    }

    public override void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;
        _dragArea.relativePosition = Vector2.zero;

        _severityImage.relativePosition = new Vector3(_padding, _padding);
        _titleRenderMaxSize = new Vector2(width - 2 * _padding - 10 - _severityImage.width, 4096);

        if (!string.IsNullOrEmpty(_title)) {
            using (var textRenderer = RentTitleRenderer()) {
                _titleRenderSize = textRenderer.GetRenderedSize(_title);
            }
        }
        else {
            _titleRenderSize = Vector2.zero;
        }

        _titleLayoutOffset = new Vector3(_padding + _severityImage.width + 20, _padding);

        if (!string.IsNullOrEmpty(_message)) {
            using (var textRenderer = RentMessageRenderer()) {
                _messageRenderSize = textRenderer.GetRenderedSize(_message);
                _messageRenderSize = _messageRenderSize.ClampMax(new Vector2(width - _padding * 3, 4096));
            }
        }
        else {
            _messageRenderSize = Vector2.zero;
        }

        _messageLayoutOffset = new Vector3(_titleLayoutOffset.x, _titleLayoutOffset.y + _titleRenderSize.y + 20);

        var severityImageBottom = _severityImage.relativePosition.y + _severityImage.height;
        var messageBottom = _messageLayoutOffset.y + _messageRenderSize.y;

        var dividerY = severityImageBottom > messageBottom ? severityImageBottom + 20 : messageBottom + 20;
        _dragArea.height = dividerY;

        _scrollViewer.relativePosition = new Vector3(_padding, dividerY);
        _buttonsYOffset = _scrollViewer.relativePosition.y + _scrollViewer.height + 20;

        ArrangeButtonAndSetHeight();

        IsPerformingAutoLayout = false;
    }

    protected override DynamicFontRenderer RentTitleRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = false;
        fontRenderer.MaxSize = _titleRenderMaxSize;
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _titleTextScale;
        fontRenderer.LayoutOffset = _titleLayoutOffset;
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.DefaultColor = UIColorPalette.White;
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended) {
            PerformAutoLayout();
            CenterToParent();
        }

        base.OnSizeChanged();
    }

    private void OnCopy() {
        if (string.IsNullOrEmpty(_details)) return;

        GUIUtility.systemCopyBuffer = _details;
    }

    private void InitDetailsTextField(string details) {
        if (string.IsNullOrEmpty(details)) {
            if (_detailsTextField == null) return;
        }
        else {
            if (_detailsTextField == null) {
                _detailsTextField = _contentContainer.AddUIComponent<TextField>();
                _detailsTextField.SuspendAutoLayout();
                _detailsTextField.ReadOnly = true;
                _detailsTextField.TextLineSpacing = 8;
                _detailsTextField.TextHorizontalAlignment = UIHorizontalAlignment.Left;
                _detailsTextField.TextWordWrap = true;
                _detailsTextField.TextScale = 0.8f;
                _detailsTextField.SizeMode = TextSizeMode.FitContent;
                _detailsTextField.SetSizeIfChanged(new Vector2(_contentContainer.width, 20));
                _detailsTextField.SelectionAtlas = DefaultAtlas.Instance;
                _detailsTextField.SelectionSprite = DefaultAtlas.Rectangle;
                _detailsTextField.TextPadding = new EdgeInsets(10);
                _detailsTextField.ResumeAutoLayout();
            }
        }

        _detailsTextField.SetTextWithoutNotify(details);
    }

    public static void ShowExceptionDialog(ModDomain modDomain, string message, string details = null, ExceptionDialogSeverity severity = ExceptionDialogSeverity.Error, Action<ExceptionDialog> setup = null) => Show<ExceptionDialog>(dialog => {
        dialog.Title = modDomain.ModSystem?.ModName ?? string.Empty;
        dialog.Severity = severity;
        dialog.Message = message;
        dialog.Details = details;
        if (modDomain.ModSystem != null) {
            dialog._buttons[0].Text = modDomain.ModSystem.ModLocalizationSystem.GetString(LocalizationStrings.OK);
            dialog._buttons[1].Text = modDomain.ModSystem.ModLocalizationSystem.GetString(LocalizationStrings.Copy);
        }

        setup?.Invoke(dialog);
    });

    public static void ShowExceptionDialog(string title, string message, string details = null, ExceptionDialogSeverity severity = ExceptionDialogSeverity.Error, Action<ExceptionDialog> setup = null) => Show<ExceptionDialog>(dialog => {
        dialog.Title = title;
        dialog.Severity = severity;
        dialog.Message = message;
        dialog.Details = details;
        setup?.Invoke(dialog);
    });
}