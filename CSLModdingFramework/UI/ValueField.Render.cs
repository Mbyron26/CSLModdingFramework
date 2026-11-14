using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public partial class ValueField {
    protected BackgroundRenderer _background;
    protected UIRenderData _selectionRenderData;
    protected bool _isCursorLayoutDirty;
    protected IAtlas _selectionAtlas = DefaultAtlas.Instance;
    protected string _selectionSprite = DefaultAtlas.Rectangle;
    protected Color32 _selectionColor = new(34, 42, 58, 255);
    protected ThemeKind _theme;

    public virtual ThemeKind Theme {
        get => _theme;
        set {
            if (_theme == value) return;

            _theme = value;
            var bg = Background;
            bg.Atlas = DefaultAtlas.Instance;
            bg.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
            bg.Colors.SetValues(_theme == ThemeKind.ClassicDark ? UIColorPalette.ClassicDarkPalette.SettingItemControlColors : UIColorPalette.ClassicLightPalette.SettingItemControlColors);
        }
    }

    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);

    public Color32 SelectionColor {
        get => _selectionColor;
        set {
            if (_selectionColor.EqualsColor(value)) return;
            _selectionColor = value;
            Invalidate();
        }
    }

    public string SelectionSprite {
        get => _selectionSprite;
        set {
            if (string.Equals(value, _selectionSprite)) return;
            _selectionSprite = value;
            Invalidate();
        }
    }

    public IAtlas SelectionAtlas {
        get => _selectionAtlas;
        set {
            if (Equals(value, _selectionAtlas)) return;
            _selectionAtlas = value;
            Invalidate();
        }
    }

    public override void PerformAutoLayout() {
        var finalWidth = m_Size.x;
        var finalHeight = m_Size.y;

        var textSize = MeasureRenderedTextSize(finalWidth - _textPadding.Horizontal, finalHeight > 4096 ? finalHeight : 4096, _textWordWrap);

        _textRenderMaxWidth = Mathf.Clamp(textSize.x, 0, finalWidth);
        _textRenderMaxHeight = Mathf.Clamp(textSize.y, 0, finalHeight);

        var finalTextOffsetX = _textHorizontalAlignment switch {
            UIHorizontalAlignment.Left => _textPadding.Left,
            UIHorizontalAlignment.Right => finalWidth - _textRenderMaxWidth - _textPadding.Right,
            _ => (finalWidth - _textRenderMaxWidth) / 2
        };

        var finalTextOffsetY = _textVerticalAlignment switch {
            UIVerticalAlignment.Top => _textPadding.Top,
            UIVerticalAlignment.Bottom => finalHeight - _textRenderMaxHeight - _textPadding.Bottom,
            _ => (finalHeight - _textRenderMaxHeight) / 2
        };

        _textRenderPosition = new Vector2(finalTextOffsetX, finalTextOffsetY);
    }

    public override DynamicFontRenderer RentTextRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.CharacterSpacing = _textCharacterSpacing;
        fontRenderer.WordWrap = _textWordWrap;
        fontRenderer.LineSpacing = _textLineSpacing;
        fontRenderer.SpaceAdditionalWidth = _textSpaceAdditionalWidth;
        fontRenderer.MaxSize = new Vector2(m_Size.x - _textPadding.Horizontal, m_Size.y - _textPadding.Vertical);
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _textScale;
        fontRenderer.LayoutOffset = _textRenderPosition;
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = _textProcessMarkup;
        fontRenderer.DefaultColor = GetTextColor();
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();

        if (isLayoutSuspended) return;

        if (Font is not { IsValid: true }) return;

        if (_selectionRenderData is not null) {
            _selectionRenderData.Clear();
        }
        else {
            _selectionRenderData = UIRenderData.Obtain();
            RenderData.Add(_selectionRenderData);
        }

        if (_textRenderData != null) {
            _textRenderData.Clear();
        }
        else {
            _textRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_textRenderData);
        }

        var text = GetRenderText();
        if (string.IsNullOrEmpty(text)) return;

        using var fontRenderer = RentTextRenderer();
        fontRenderer.PerformTextLayout(text);

        if (fontRenderer.Lines.Count > 0) {
            _lineHeight = fontRenderer.Lines.First().Height;
        }
        else {
            _lineHeight = 0;
        }

        ReleaseTextLineListItems();

        foreach (var textLine in fontRenderer.Lines) {
            _lines.Add(TextLine.Rent().CopyFrom(textLine));
        }

        if (_lines.Count > 0) {
            var paddedHeight = m_Size.y - _textPadding.Vertical;
            var lineAdvance = _lineHeight + _textLineSpacing;
            var maxLines = Mathf.FloorToInt((paddedHeight + _textLineSpacing) / lineAdvance);
            maxLines = Mathf.Max(1, maxLines);
            if (_lines.Count > maxLines) {
                for (var i = maxLines; i < _lines.Count; i++) {
                    _lines[i].Return();
                }

                _lines.RemoveRange(maxLines, _lines.Count - maxLines);
            }
        }

        if (_selectionAtlas is not null)
            _selectionRenderData.material = _selectionAtlas.Material;

        var composition = Input.compositionString;
        var renderCursorIndex = CursorIndex;

        if (!string.IsNullOrEmpty(composition))
            renderCursorIndex += composition.Length;

        if (_isCursorLayoutDirty) {
            UpdateCursorLayout(renderCursorIndex);
            _isCursorLayoutDirty = false;
        }

        RenderCursor();
        RenderSelection();
        fontRenderer.Render(_textRenderData);
    }

    public void ApplyStyle(ThemeKind style) {
        _selectionAtlas = DefaultAtlas.Instance;
        _selectionSprite = DefaultAtlas.Rectangle;

        Background.Atlas = DefaultAtlas.Instance;
        Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);

        Background.Colors.SetValues(style == ThemeKind.ClassicLight ? UIColorPalette.ClassicLightPalette.SettingItemControlColors : UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
    }

    private string GetRenderText() {
        var composition = Input.compositionString;

        if (string.IsNullOrEmpty(composition))
            return _text;

        return _text.Insert(CursorIndex, composition);
    }


    private void UpdateCursorLayout(int cursorIndex) {
        if (_lines.Count == 0) {
            _cursorX = 0f;
            _cursorY = 0f;
            return;
        }

        for (var i = 0; i < _lines.Count; i++) {
            var line = _lines[i];

            if (cursorIndex >= line.StartIndex && cursorIndex <= line.EndIndex) {
                var lineCursorIndex = cursorIndex - line.StartIndex;

                _cursorX = _textRenderPosition.x + line.CharOffsets[lineCursorIndex];
                _cursorY = _textRenderPosition.y
                           + i * (line.Height + TextLineSpacing);

                return;
            }
        }

        var lastLine = _lines[_lines.Count - 1];

        _cursorX = _textRenderPosition.x
                   + lastLine.CharOffsets[lastLine.CharCount];

        _cursorY = _textRenderPosition.y
                   + (_lines.Count - 1) * (lastLine.Height + TextLineSpacing);
    }

    private void RenderSelection() {
        if (!HasSelection || _selectionAtlas == null || string.IsNullOrEmpty(_selectionSprite) || !_selectionAtlas.TryGetSprite(_selectionSprite, out int spriteIndex))
            return;

        var unitsPerPixel = UnitsPerPixel;
        var lineHeight = _lineHeight * unitsPerPixel;

        for (var i = 0; i < _lines.Count; i++) {
            var line = _lines[i];

            var lineStart = Mathf.Max(line.StartIndex, SelectionStart);
            var lineEnd = Mathf.Min(line.EndIndex, SelectionEnd);

            if (lineStart >= lineEnd) continue;

            var leftOffset = lineStart - line.StartIndex;
            var rightOffset = lineEnd - line.StartIndex;

            var leftX = line.CharOffsets[leftOffset];
            var rightX = line.CharOffsets[rightOffset];

            var worldLeftX = (_textRenderPosition.x + leftX) * unitsPerPixel;
            var worldRightX = (_textRenderPosition.x + rightX) * unitsPerPixel;

            var worldTopY = -(_textRenderPosition.y + i * (line.Height + TextLineSpacing)) * unitsPerPixel;

            var worldBottomY = worldTopY - lineHeight;

            RenderSelectionQuad(worldLeftX, worldTopY, worldRightX, worldBottomY, spriteIndex);
        }
    }

    private void RenderSelectionQuad(float left, float top, float right, float bottom, int spriteIndex) {
        AddTriangles(_selectionRenderData.triangles, _selectionRenderData.vertices.Count);
        var vector = new Vector3(left, top);
        var vector2 = new Vector3(right, top);
        var vector3 = new Vector3(left, bottom);
        var vector4 = new Vector3(right, bottom);
        _selectionRenderData.vertices.Add(vector);
        _selectionRenderData.vertices.Add(vector2);
        _selectionRenderData.vertices.Add(vector4);
        _selectionRenderData.vertices.Add(vector3);

        _selectionRenderData.colors.Add(_selectionColor);
        _selectionRenderData.colors.Add(_selectionColor);
        _selectionRenderData.colors.Add(_selectionColor);
        _selectionRenderData.colors.Add(_selectionColor);

        ref readonly var spriteMetadata = ref _selectionAtlas.GetSprite(spriteIndex);

        var region = spriteMetadata.Region;
        var num = region.width / spriteMetadata.PixelSize.x;
        var num2 = region.height / spriteMetadata.PixelSize.y;
        _selectionRenderData.uvs.Add(new Vector2(region.x + num, region.yMax - num2));
        _selectionRenderData.uvs.Add(new Vector2(region.xMax - num, region.yMax - num2));
        _selectionRenderData.uvs.Add(new Vector2(region.xMax - num, region.y + num2));
        _selectionRenderData.uvs.Add(new Vector2(region.x + num, region.y + num2));
    }

    private void RenderCursor() {
        if (!_cursorShown || HasSelection || _selectionAtlas == null || string.IsNullOrEmpty(_selectionSprite)) return;
        if (!_selectionAtlas.TryGetSprite(_selectionSprite, out int index)) return;

        ref readonly var spriteMetadata = ref _selectionAtlas.GetSprite(index);

        var unitsPerPixel = UnitsPerPixel;
        var left = (_cursorX * unitsPerPixel).Quantize(unitsPerPixel);
        var top = -_cursorY * unitsPerPixel;
        var cursorWidth = CursorWidth * unitsPerPixel;
        var cursorHeight = _lineHeight * unitsPerPixel;
        var bottom = top - cursorHeight;
        var tl = new Vector3(left, top);
        var tr = new Vector3(left + cursorWidth, top);
        var bl = new Vector3(left, bottom);
        var br = new Vector3(left + cursorWidth, bottom);
        var vertices = _selectionRenderData.vertices;
        var triangles = _selectionRenderData.triangles;
        var uvs = _selectionRenderData.uvs;
        var colors = _selectionRenderData.colors;
        AddTriangles(triangles, vertices.Count);
        vertices.Add(tl);
        vertices.Add(tr);
        vertices.Add(br);
        vertices.Add(bl);
        var renderColor = TextColors.GetValue(_state);
        colors.Add(renderColor);
        colors.Add(renderColor);
        colors.Add(renderColor);
        colors.Add(renderColor);
        var region = spriteMetadata.Region;
        uvs.Add(new Vector2(region.x, region.yMax));
        uvs.Add(new Vector2(region.xMax, region.yMax));
        uvs.Add(new Vector2(region.xMax, region.y));
        uvs.Add(new Vector2(region.x, region.y));
    }

    private void AddTriangles(PoolList<int> triangles, int baseIndex) {
        for (var i = 0; i < SpriteMeshBuilder.QuadTriangleIndices.Length; i++) {
            triangles.Add(SpriteMeshBuilder.QuadTriangleIndices[i] + baseIndex);
        }
    }

    private void SetCursorLayoutDirty() => _isCursorLayoutDirty = true;
}