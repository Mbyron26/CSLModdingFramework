using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;

namespace CSLModdingFramework.UI.Controls;

public class TextLabel : TextControl {
    protected BackgroundRenderer _background;
    protected ForegroundRenderer _foreground;
    protected string _prefixText = string.Empty;
    protected string _suffixText = string.Empty;

    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);
    public ForegroundRenderer Foreground => _foreground ??= new ForegroundRenderer(this);
    public string FullText => _prefixText + _text + _suffixText;

    public string PrefixText {
        get => _prefixText;
        set {
            if (string.Equals(value, _prefixText)) return;

            _prefixText = value;
            InvalidateAutoLayout();
        }
    }

    public string SuffixText {
        get => _suffixText;
        set {
            if (string.Equals(value, _suffixText)) return;

            _suffixText = value;
            InvalidateAutoLayout();
        }
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
        RenderText();
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

        if (IsAutoLayoutSuspended || string.IsNullOrEmpty(FullText)) return;

        using var fontRenderer = RentTextRenderer();
        fontRenderer.Render(FullText, _textRenderData);
    }

    protected override void OnSizeChanged() {
        PerformAutoLayout();

        base.OnSizeChanged();
    }
}