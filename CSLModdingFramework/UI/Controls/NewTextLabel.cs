using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class NewTextLabel : TextControl {
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

    public override Vector2 MeasureRenderedTextSize(float maxWidth = 4096, float maxHeight = 4096, bool allowWordWrap = false) {
        if (string.IsNullOrEmpty(FullText)) return Vector2.zero;

        using var uiFontRenderer = Font.RentRenderer();
        uiFontRenderer.WordWrap = allowWordWrap;
        uiFontRenderer.MaxSize = new Vector2(maxWidth, maxHeight);
        uiFontRenderer.PixelRatio = PixelsToUnits();
        uiFontRenderer.TextScale = _textScale;
        uiFontRenderer.LayoutOffset = Vector3.zero;
        uiFontRenderer.TextAlign = UIHorizontalAlignment.Left;
        uiFontRenderer.ProcessMarkup = _textProcessMarkup;
        uiFontRenderer.DefaultColor = UIColorPalette.White;
        return uiFontRenderer.GetRenderedSize(FullText);
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
        RenderText();
    }

    protected override void RenderText() {
        if (Font == null || !Font.IsValid) return;

        if (_textRenderData != null) {
            _textRenderData.Clear();
        }
        else {
            _textRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_textRenderData);
        }

        if (isLayoutSuspended || string.IsNullOrEmpty(FullText)) return;

        using var uiFontRenderer = RentTextRenderer();
        uiFontRenderer.Render(FullText, _textRenderData);
    }
}