using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class TextField : ValueField {
    public override void PerformAutoLayout() {
        IsPerformingAutoLayout = true;
        float finalHeight;
        float finalTextOffsetX;
        float finalTextOffsetY;
        if (_sizeMode == TextSizeMode.FitContent) {
            var fixedWidth = m_Size.x;
            var availableWidth = fixedWidth - _textPadding.Horizontal;

            var textSize = MeasureRenderedTextSize(availableWidth, allowWordWrap: _textWordWrap);
            _textRenderMaxWidth = Mathf.Clamp(textSize.x, 0, availableWidth);
            _textRenderMaxHeight = textSize.y;

            finalHeight = Mathf.Clamp(_textRenderMaxHeight + _textPadding.Vertical, _minSize.Height, _maxSize.Height > 0 ? _maxSize.Height : 4096
            );

            finalTextOffsetX = _textHorizontalAlignment switch {
                UIHorizontalAlignment.Left => _textPadding.Left,
                UIHorizontalAlignment.Right => fixedWidth - _textRenderMaxWidth - _textPadding.Right,
                _ => (fixedWidth - _textRenderMaxWidth) / 2
            };

            finalTextOffsetY = _textVerticalAlignment switch {
                UIVerticalAlignment.Top => _textPadding.Top,
                UIVerticalAlignment.Bottom => finalHeight - _textRenderMaxHeight - _textPadding.Bottom,
                _ => (finalHeight - _textRenderMaxHeight) / 2
            };

            SetHeightIfChanged(finalHeight);
        }
        else {
            var finalWidth = m_Size.x;
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

            SetSizeInternal(new Vector2(finalWidth, finalHeight));
        }

        _textRenderPosition = new Vector2(finalTextOffsetX, finalTextOffsetY);
        IsPerformingAutoLayout = false;
    }
}