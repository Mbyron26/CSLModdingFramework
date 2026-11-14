using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using CSLModdingFramework.Collections;
using UnityEngine;

namespace CSLModdingFramework.UI.Fonts;

public sealed class DynamicFontRenderer : IDisposable {
    internal static readonly Stack<DynamicFontRenderer> Pool = new();
    private static readonly int[] TriangleIndices = { 0, 1, 3, 3, 1, 2 };
    private readonly Stack<Color32> _colorStack = new();
    private List<MarkupToken> _tokens;
    private bool _hasLayout;

    public DynamicFont Font { get; private set; }
    public Vector2 MaxSize { get; set; }
    public float PixelRatio { get; set; }
    public float TextScale { get; set; } = 1f;
    public float CharacterSpacing { get; set; }
    public float LineSpacing { get; set; } = 0.8f;

    /// <summary>
    /// Position offset in UI coordinate space.
    /// Values are specified in UI pixels and should not be manually scaled or Y-flipped.
    /// </summary>
    public Vector3 LayoutOffset { get; set; }

    /// <summary>
    /// Additional width applied to space characters.
    /// </summary>
    public float SpaceAdditionalWidth { get; set; } = 1f;

    public bool ProcessMarkup { get; set; }
    public bool WordWrap { get; set; }
    public int MinTokenSplitWidth { get; set; } = 60;

    public UIHorizontalAlignment TextAlign { get; set; }
    public Color32 DefaultColor { get; set; } = new(255, 255, 255, 255);
    public float Opacity { get; set; }
    public int TabSize { get; set; } = 32;
    public Vector2 RenderedSize { get; internal set; }
    public List<TextLine> Lines { get; private set; }

    private DynamicFontRenderer() { }

    public float GetCharacterHeight => Font.Baseline * TextScale;

    public float[] MeasureCharacterAdvances(string text, out float totalWidth) => MeasureCharacterAdvances(text, 0, text.Length, out totalWidth);

    public float[] MeasureCharacterAdvances(string text, int startIndex, int endIndex, out float totalWidth) {
        totalWidth = 0f;

        var count = endIndex - startIndex;
        var widths = new float[count];

        for (var i = startIndex; i < endIndex; i++) {
            var c = text[i];

            float advance;

            if (c == '\t') {
                advance = TabSize;
            }
            else {
                Font.GetCharacterInfo(c, out var info, TextScale);
                advance = info.advance;

                if (c == ' ')
                    advance += SpaceAdditionalWidth;
            }

            widths[i - startIndex] = advance;
            totalWidth += advance;
        }

        return widths;
    }

    public Vector2 GetRenderedSize(string text) {
        PerformTextLayout(text);
        var renderedWidth = 0f;
        var renderedHeight = 0f;
        for (var i = 0; i < Lines.Count; i++) {
            renderedWidth = Mathf.Max(Lines[i].CharOffsets[Lines[i].CharCount], renderedWidth);
            renderedHeight += Lines[i].Height;
            if (i > 0)
                renderedHeight += LineSpacing;
        }

        if (_tokens != null) {
            for (var i = 0; i < _tokens.Count; i++) {
                _tokens[i].Return();
            }

            ListPool<MarkupToken>.Return(_tokens);
            _tokens = null;
        }

        if (Lines != null) {
            for (var i = 0; i < Lines.Count; i++) {
                Lines[i].Return();
            }

            ListPool<TextLine>.Return(Lines);
            Lines = null;
        }

        return new Vector2(renderedWidth, renderedHeight);
    }

    public void Reset() {
        Font = null;
        MaxSize = Vector2.zero;
        PixelRatio = 0f;
        TextScale = 1f;
        CharacterSpacing = 0f;
        LineSpacing = 0.8f;
        LayoutOffset = Vector3.zero;
        ProcessMarkup = false;
        WordWrap = false;
        TextAlign = UIHorizontalAlignment.Left;
        DefaultColor = Color.white;
        Opacity = 1f;
        SpaceAdditionalWidth = 1;
        TabSize = 32;
        MinTokenSplitWidth = 60;
        _hasLayout = false;
        if (_tokens != null) {
            for (var i = 0; i < _tokens.Count; i++) {
                _tokens[i].Return();
            }

            ListPool<MarkupToken>.Return(_tokens);
            _tokens = null;
        }

        if (Lines != null) {
            for (var i = 0; i < Lines.Count; i++) {
                Lines[i].Return();
            }

            ListPool<TextLine>.Return(Lines);
            Lines = null;
        }

        _colorStack.Clear();
        Pool.Push(this);
    }

    public void Dispose() => Reset();

    public void Render(string text, UIRenderData destination) {
        PerformTextLayout(text);
        Render(destination);
    }

    public void Render(UIRenderData destination) {
        if (!_hasLayout) return;

        var renderX = 0;
        var renderY = 0f;

        var renderFrom = LayoutOffset.Scale(1, -1, 0);
        var linesHeight = 0f;

        for (var i = 0; i < Lines.Count; i++) {
            var textLine = Lines[i];

            linesHeight += textLine.Height;
            if (linesHeight > MaxSize.y) break;

            var startIndex = destination.vertices.Count;
            RenderLine(Lines[i], renderFrom, destination);
            renderFrom.y -= textLine.Height + LineSpacing;
            renderX = Mathf.Max((int)textLine.Width, renderX);
            renderY += textLine.Height;
            if (textLine.Width > MaxSize.x) {
                ClipRight(destination, startIndex);
            }

            ClipBottom(destination, startIndex);
        }

        RenderedSize = new Vector2(Mathf.Min(MaxSize.x, renderX), Mathf.Min(MaxSize.y, renderY)) * TextScale;
    }

    public void PerformTextLayout(string text) {
        if (Font == null) {
            throw new InvalidOperationException("Font is not set.");
        }

        var scaledFontSize = Mathf.CeilToInt(Font.FontSize * TextScale);
        Font.RequestCharacters(text, scaledFontSize, FontStyle.Normal);
        if (_tokens == null) {
            _tokens = ListPool<MarkupToken>.Rent(128);
        }
        else {
            for (var i = 0; i < _tokens.Count; i++) {
                _tokens[i].Return();
            }

            _tokens.Clear();
        }

        if (ProcessMarkup) {
            MarkupTextTokenizer.Tokenize(text, _tokens);
        }
        else {
            PlainTextTokenizer.Tokenize(text, _tokens);
        }

        for (var i = 0; i < _tokens.Count; i++) {
            CalculateTokenRenderSize(_tokens[i]);
        }

        CalculateLineBreaks(text);
        _hasLayout = true;
    }

    private void RenderLine(TextLine line, Vector3 position, UIRenderData destination) {
        RestoreColorState(line);
        position.x += CalculateLineAlignment(line);

        for (var i = line.StartToken; i < line.EndToken; i++) {
            var token = _tokens[i];

            switch (token.Kind) {
                case MarkupTokenKind.Text:
                    position.x += RenderText(token, line, position, destination);
                    break;

                case MarkupTokenKind.Whitespace: {
                    var start = Math.Max(token.StartIndex, line.StartIndex);
                    var end = Math.Min(token.EndIndex, line.EndIndex);

                    for (var index = start; index < end; index++) {
                        var offset = index - token.StartIndex;
                        position.x += token.CharacterAdvances[offset];
                    }

                    break;
                }

                case MarkupTokenKind.ColorStart:
                    PushColor(token);
                    break;

                case MarkupTokenKind.ColorEnd:
                    PopColor();
                    break;
            }
        }
    }

    private float RenderText(MarkupToken token, TextLine line, Vector3 position, UIRenderData renderData) {
        var fontSize = Mathf.CeilToInt(Font.FontSize * TextScale);

        var start = Math.Max(token.StartIndex, line.StartIndex);
        var end = Math.Min(token.EndIndex, line.EndIndex);

        var vertices = renderData.vertices;
        var triangles = renderData.triangles;
        var uvs = renderData.uvs;
        var colors = renderData.colors;

        var x = position.x;
        var y = position.y;

        var spacing = CharacterSpacing * TextScale;
        renderData.material = Font.FontMaterial;
        var color = ApplyOpacity(_colorStack.Count == 0 ? DefaultColor : _colorStack.Peek());

        for (var index = start; index < end; index++) {
            var offset = index - token.StartIndex;
            var c = token[offset];

            if (Font.BaseFont.GetCharacterInfo(c, out var characterInfo, fontSize, FontStyle.Normal)) {
                var yOffset = Font.FontSize + characterInfo.maxY - Font.BaseFont.ascent - fontSize + Font.Descent - 1;
                var top = y + yOffset;
                var bottom = top - characterInfo.glyphHeight;

                var left = x + characterInfo.minX;
                var right = x + characterInfo.maxX;

                var v0 = new Vector3(left, top) * PixelRatio;
                var v1 = new Vector3(right, top) * PixelRatio;
                var v2 = new Vector3(right, bottom) * PixelRatio;
                var v3 = new Vector3(left, bottom) * PixelRatio;

                AddTriangleIndices(vertices, triangles);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);

                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);

                AddUVCoords(uvs, characterInfo);
                x += characterInfo.advance;

                if (index < end - 1)
                    x += spacing;
            }
        }

        return x - position.x;
    }

    private static void AddUVCoords(PoolList<Vector2> uvs, CharacterInfo glyph) {
        uvs.Add(glyph.uvTopLeft);
        uvs.Add(glyph.uvTopRight);
        uvs.Add(glyph.uvBottomRight);
        uvs.Add(glyph.uvBottomLeft);
    }

    private int CalculateLineAlignment(TextLine line) {
        var lineWidth = line.Width;
        if (TextAlign == UIHorizontalAlignment.Left || lineWidth < 1f) {
            return 0;
        }

        float num;
        if (TextAlign == UIHorizontalAlignment.Right) {
            num = MaxSize.x - lineWidth;
        }
        else {
            num = (MaxSize.x - lineWidth) * 0.5f;
        }

        return Mathf.CeilToInt(Mathf.Max(0f, num));
    }

    private void PushColor(MarkupToken token) => _colorStack.Push(token.Color);

    private void PopColor() {
        if (_colorStack.Count > 0)
            _colorStack.Pop();
    }

    private void RestoreColorState(TextLine line) {
        _colorStack.Clear();

        for (var i = 0; i < line.StartToken; i++) {
            var token = _tokens[i];

            if (token.Kind == MarkupTokenKind.ColorStart) {
                _colorStack.Push(token.Color);
            }
            else if (token.Kind == MarkupTokenKind.ColorEnd) {
                if (_colorStack.Count > 0)
                    _colorStack.Pop();
            }
        }
    }

    private void ClipRight(UIRenderData destination, int startIndex) {
        if (destination == null) return;

        var clipRight = (LayoutOffset.x + MaxSize.x) * PixelRatio;
        var vertices = destination.vertices;
        var uvs = destination.uvs;

        for (var vertexIndex = startIndex; vertexIndex < vertices.Count; vertexIndex += 4) {
            var topLeft = vertices[vertexIndex];
            var topRight = vertices[vertexIndex + 1];
            var bottomRight = vertices[vertexIndex + 2];
            var bottomLeft = vertices[vertexIndex + 3];

            var quadWidth = topRight.x - topLeft.x;

            if (topRight.x <= clipRight) {
                continue;
            }

            var visibleRatio = 1f - (clipRight - topRight.x + quadWidth) / quadWidth;

            topLeft.x = Mathf.Min(topLeft.x, clipRight);
            topRight.x = Mathf.Min(topRight.x, clipRight);
            bottomRight.x = Mathf.Min(bottomRight.x, clipRight);
            bottomLeft.x = Mathf.Min(bottomLeft.x, clipRight);

            vertices[vertexIndex] = topLeft;
            vertices[vertexIndex + 1] = topRight;
            vertices[vertexIndex + 2] = bottomRight;
            vertices[vertexIndex + 3] = bottomLeft;

            uvs[vertexIndex + 1] = Vector2.Lerp(
                uvs[vertexIndex + 1],
                uvs[vertexIndex],
                visibleRatio
            );

            uvs[vertexIndex + 2] = Vector2.Lerp(
                uvs[vertexIndex + 2],
                uvs[vertexIndex + 3],
                visibleRatio
            );
        }
    }

    private void ClipBottom(UIRenderData destination, int startIndex) {
        if (destination == null) {
            return;
        }

        var num = (-LayoutOffset.y - MaxSize.y) * PixelRatio;
        var vertices = destination.vertices;
        var uvs = destination.uvs;
        var colors = destination.colors;
        for (var i = startIndex; i < vertices.Count; i += 4) {
            var vector = vertices[i];
            var vector2 = vertices[i + 1];
            var vector3 = vertices[i + 2];
            var vector4 = vertices[i + 3];
            var num2 = vector.y - vector4.y;
            if (vector4.y <= num) {
                var num3 = 1f - Mathf.Abs(-num + vector.y) / num2;
                var poolList = vertices;
                var num4 = i;
                vector = new Vector3(vector.x, Mathf.Max(vector.y, num), vector2.z);
                poolList[num4] = vector;
                var poolList2 = vertices;
                var num5 = i + 1;
                vector2 = new Vector3(vector2.x, Mathf.Max(vector2.y, num), vector2.z);
                poolList2[num5] = vector2;
                var poolList3 = vertices;
                var num6 = i + 2;
                vector3 = new Vector3(vector3.x, Mathf.Max(vector3.y, num), vector3.z);
                poolList3[num6] = vector3;
                var poolList4 = vertices;
                var num7 = i + 3;
                vector4 = new Vector3(vector4.x, Mathf.Max(vector4.y, num), vector4.z);
                poolList4[num7] = vector4;
                uvs[i + 3] = Vector2.Lerp(uvs[i + 3], uvs[i], num3);
                uvs[i + 2] = Vector2.Lerp(uvs[i + 2], uvs[i + 1], num3);
                var color = Color32.Lerp(colors[i + 3], colors[i], num3);
                colors[i + 3] = color;
                colors[i + 2] = color;
            }
        }
    }

    private Color32 ApplyOpacity(Color32 color) {
        color.a = (byte)Mathf.RoundToInt(Mathf.Clamp01(Opacity) * 255f);
        return color;
    }

    private static void AddTriangleIndices(PoolList<Vector3> verts, PoolList<int> triangles) {
        foreach (var t in TriangleIndices) {
            triangles.Add(verts.Count + t);
        }
    }

    #region NewTokenWrap

    private void CalculateLineBreaks(string text) {
        if (Lines == null) {
            Lines = ListPool<TextLine>.Rent(32);
        }
        else {
            for (var i = 0; i < Lines.Count; i++) {
                Lines[i].Return();
            }

            Lines.Clear();
        }

        var spacing = CharacterSpacing * TextScale;
        var maxWidth = MaxSize.x;
        var height = Font.Baseline * TextScale;
        var minTokenSplitWidth = MinTokenSplitWidth * TextScale;

        var lineStart = 0;
        var lineStartToken = 0;
        var lineWidth = 0f;
        var tokenIndex = 0;


        while (tokenIndex < _tokens.Count) {
            var token = _tokens[tokenIndex];

            if (token.Kind == MarkupTokenKind.Newline) {
                Lines.Add(TextLine.Rent(lineStart, token.StartIndex, lineStartToken, tokenIndex, lineWidth, height));

                var line = Lines[Lines.Count - 1];
                FillLineCharWidths(line);
                FillLineCharOffsets(line);

                lineStart = token.EndIndex;
                lineStartToken = tokenIndex + 1;
                lineWidth = 0f;

                tokenIndex++;
                continue;
            }


            if (token.Kind != MarkupTokenKind.Text && token.Kind != MarkupTokenKind.Whitespace) {
                tokenIndex++;
                continue;
            }

            if (!WordWrap || lineWidth + token.RenderWidth <= maxWidth) {
                lineWidth += token.RenderWidth;
                tokenIndex++;
                continue;
            }

            if (token.RenderWidth <= minTokenSplitWidth) {
                Lines.Add(TextLine.Rent(lineStart, token.StartIndex, lineStartToken, tokenIndex, lineWidth, height));

                var line = Lines[Lines.Count - 1];
                FillLineCharWidths(line);
                FillLineCharOffsets(line);

                lineStart = token.StartIndex;
                lineStartToken = tokenIndex;
                lineWidth = 0f;

                continue;
            }

            var charOffset = 0;
            var charLineWidth = 0f;


            while (charOffset < token.Length) {
                var charWidth = token.CharacterAdvances[charOffset];

                if (charLineWidth > 0) {
                    charWidth += spacing;
                }


                if (lineWidth + charLineWidth + charWidth > maxWidth && lineWidth + charLineWidth > 0) {
                    var endIndex = token.StartIndex + charOffset;

                    var line = TextLine.Rent(lineStart, endIndex, lineStartToken, tokenIndex + 1, lineWidth + charLineWidth, height);

                    FillLineCharWidths(line);
                    FillLineCharOffsets(line);

                    Lines.Add(line);

                    lineStart = endIndex;
                    lineStartToken = tokenIndex;

                    lineWidth = 0f;
                    charLineWidth = 0f;

                    continue;
                }


                charLineWidth += charWidth;
                charOffset++;
            }


            lineWidth += charLineWidth;
            tokenIndex++;
        }


        if (lineStart < text.Length || Lines.Count == 0) {
            Lines.Add(TextLine.Rent(lineStart, text.Length, lineStartToken, _tokens.Count, lineWidth, height));

            var line = Lines[Lines.Count - 1];
            FillLineCharWidths(line);
            FillLineCharOffsets(line);
        }
    }

    #endregion

    #region TokenWrap

    /*
     * Wraps text only between tokens. A token is moved to the next line if it cannot fit.
     */
    // private void CalculateLineBreaks(string text) {
    //     if (Lines == null) {
    //         Lines = ListPool<TextLine>.Rent(32);
    //     }
    //     else {
    //         for (var i = 0; i < Lines.Count; i++) {
    //             Lines[i].Return();
    //         }
    //
    //         Lines.Clear();
    //     }
    //
    //     var spacing = CharacterSpacing * TextScale;
    //     var maxWidth = MaxSize.x;
    //     var height = Font.Baseline * TextScale;
    //
    //     var lineStart = 0;
    //     var lineStartToken = 0;
    //     var lineWidth = 0f;
    //     var tokenIndex = 0;
    //
    //     while (tokenIndex < _tokens.Count) {
    //         var token = _tokens[tokenIndex];
    //
    //         if (token.Kind == MarkupTokenKind.Newline) {
    //             Lines.Add(TextLine.Rent(
    //                 lineStart,
    //                 token.StartIndex,
    //                 lineStartToken,
    //                 tokenIndex,
    //                 lineWidth,
    //                 height
    //             ));
    //
    //             var line = Lines[Lines.Count - 1];
    //             FillLineCharWidths(line);
    //             FillLineCharOffsets(line);
    //
    //             lineStart = token.EndIndex;
    //             lineStartToken = tokenIndex + 1;
    //             lineWidth = 0f;
    //
    //             tokenIndex++;
    //             continue;
    //         }
    //
    //         if (token.Kind != MarkupTokenKind.Text &&
    //             token.Kind != MarkupTokenKind.Whitespace) {
    //             tokenIndex++;
    //             continue;
    //         }
    //
    //         if (!WordWrap || lineWidth + token.RenderWidth <= maxWidth) {
    //             lineWidth += token.RenderWidth;
    //             tokenIndex++;
    //             continue;
    //         }
    //
    //         if (token.Kind == MarkupTokenKind.Whitespace) {
    //             Lines.Add(TextLine.Rent(
    //                 lineStart,
    //                 token.StartIndex,
    //                 lineStartToken,
    //                 tokenIndex,
    //                 lineWidth,
    //                 height
    //             ));
    //
    //             var line = Lines[Lines.Count - 1];
    //             FillLineCharWidths(line);
    //             FillLineCharOffsets(line);
    //
    //             lineStart = token.EndIndex;
    //             lineStartToken = tokenIndex + 1;
    //             lineWidth = 0f;
    //
    //             tokenIndex++;
    //             continue;
    //         }
    //
    //         if (lineWidth > 0) {
    //             Lines.Add(TextLine.Rent(
    //                 lineStart,
    //                 token.StartIndex,
    //                 lineStartToken,
    //                 tokenIndex,
    //                 lineWidth,
    //                 height
    //             ));
    //
    //             var line = Lines[Lines.Count - 1];
    //             FillLineCharWidths(line);
    //             FillLineCharOffsets(line);
    //
    //             lineStart = token.StartIndex;
    //             lineStartToken = tokenIndex;
    //             lineWidth = 0f;
    //
    //             continue;
    //         }
    //
    //
    //         var charOffset = 0;
    //         var charLineWidth = 0f;
    //
    //         while (charOffset < token.Length) {
    //             var charWidth = token.CharacterAdvances[charOffset];
    //
    //             if (charLineWidth > 0) {
    //                 charWidth += spacing;
    //             }
    //
    //             if (charLineWidth + charWidth > maxWidth && charLineWidth > 0) {
    //                 var endIndex = token.StartIndex + charOffset;
    //
    //                 var line = TextLine.Rent(
    //                     lineStart,
    //                     endIndex,
    //                     lineStartToken,
    //                     tokenIndex + 1,
    //                     charLineWidth,
    //                     height
    //                 );
    //
    //                 FillLineCharWidths(line);
    //                 FillLineCharOffsets(line);
    //                 Lines.Add(line);
    //
    //                 lineStart = endIndex;
    //                 lineStartToken = tokenIndex;
    //                 charLineWidth = 0f;
    //
    //                 continue;
    //             }
    //
    //             charLineWidth += charWidth;
    //             charOffset++;
    //         }
    //
    //         lineWidth = charLineWidth;
    //         tokenIndex++;
    //     }
    //
    //     if (lineStart < text.Length || Lines.Count == 0) {
    //         Lines.Add(TextLine.Rent(
    //             lineStart,
    //             text.Length,
    //             lineStartToken,
    //             _tokens.Count,
    //             lineWidth,
    //             height
    //         ));
    //
    //         var line = Lines[Lines.Count - 1];
    //         FillLineCharWidths(line);
    //         FillLineCharOffsets(line);
    //     }
    // }

    #endregion

    #region CharacterWrap

/*
 * Allows breaking text at any character boundary when the current line exceeds the maximum width.
 */
    // private void CalculateLineBreaks(string text) {
    //     if (Lines == null) {
    //         Lines = ListPool<TextLine>.Rent(32);
    //     }
    //     else {
    //         for (var i = 0; i < Lines.Count; i++) {
    //             Lines[i].Return();
    //         }
    //
    //         Lines.Clear();
    //     }
    //
    //     var spacing = CharacterSpacing * TextScale;
    //     var maxWidth = MaxSize.x;
    //     var height = Font.Baseline * TextScale;
    //
    //     var lineStart = 0;
    //     var lineStartToken = 0;
    //     var lineWidth = 0f;
    //     var tokenIndex = 0;
    //
    //     while (tokenIndex < _tokens.Count) {
    //         var token = _tokens[tokenIndex];
    //
    //         if (token.Kind == MarkupTokenKind.Newline) {
    //             Lines.Add(TextLine.Rent(lineStart, token.StartIndex, lineStartToken, tokenIndex, lineWidth, height));
    //             var line = Lines[Lines.Count - 1];
    //             FillLineCharWidths(line);
    //             FillLineCharOffsets(line);
    //             lineStart = token.EndIndex;
    //             lineStartToken = tokenIndex + 1;
    //             lineWidth = 0f;
    //             tokenIndex++;
    //             continue;
    //         }
    //
    //         if (token.Kind != MarkupTokenKind.Text && token.Kind != MarkupTokenKind.Whitespace) {
    //             tokenIndex++;
    //             continue;
    //         }
    //
    //         if (!WordWrap || lineWidth + token.RenderWidth <= maxWidth) {
    //             lineWidth += token.RenderWidth;
    //             tokenIndex++;
    //             continue;
    //         }
    //
    //         if (token.Kind == MarkupTokenKind.Whitespace) {
    //             Lines.Add(TextLine.Rent(lineStart, token.StartIndex, lineStartToken, tokenIndex, lineWidth, height));
    //             var line = Lines[Lines.Count - 1];
    //             FillLineCharWidths(line);
    //             FillLineCharOffsets(line);
    //             lineStart = token.EndIndex;
    //             lineStartToken = tokenIndex + 1;
    //             lineWidth = 0f;
    //
    //             tokenIndex++;
    //             continue;
    //         }
    //
    //         var charOffset = 0;
    //         var charLineWidth = 0f;
    //
    //         while (charOffset < token.Length) {
    //             var charWidth = token.CharacterAdvances[charOffset];
    //
    //             if (charLineWidth > 0) {
    //                 charWidth += spacing;
    //             }
    //
    //             if (lineWidth + charLineWidth + charWidth > maxWidth && lineWidth + charLineWidth > 0) {
    //                 var endIndex = token.StartIndex + charOffset;
    //
    //                 var line = TextLine.Rent(lineStart, endIndex, lineStartToken, tokenIndex + 1, lineWidth + charLineWidth, height);
    //                 FillLineCharWidths(line);
    //                 FillLineCharOffsets(line);
    //                 Lines.Add(line);
    //
    //                 lineStart = endIndex;
    //                 lineStartToken = tokenIndex;
    //
    //                 lineWidth = 0f;
    //                 charLineWidth = 0f;
    //
    //                 continue;
    //             }
    //
    //             charLineWidth += charWidth;
    //             charOffset++;
    //         }
    //
    //         lineWidth += charLineWidth;
    //         tokenIndex++;
    //     }
    //
    //     if (lineStart < text.Length || Lines.Count == 0) {
    //         Lines.Add(TextLine.Rent(lineStart, text.Length, lineStartToken, _tokens.Count, lineWidth, height));
    //         var line = Lines[Lines.Count - 1];
    //         FillLineCharWidths(line);
    //         FillLineCharOffsets(line);
    //     }
    // }

    #endregion

    private void FillLineCharWidths(TextLine line) {
        var charCount = line.EndIndex - line.StartIndex;
        line.CharCount = charCount;
        line.CharWidths = ArrayPool<float>.Shared.Rent(charCount);

        var writeIndex = 0;

        for (var ti = line.StartToken; ti < line.EndToken; ti++) {
            var token = _tokens[ti];

            if (token.Kind != MarkupTokenKind.Text &&
                token.Kind != MarkupTokenKind.Whitespace)
                continue;

            var start = Math.Max(token.StartIndex, line.StartIndex);
            var end = Math.Min(token.EndIndex, line.EndIndex);

            for (var ci = start; ci < end; ci++) {
                var offset = ci - token.StartIndex;
                line.CharWidths[writeIndex++] = token.CharacterAdvances[offset];
            }
        }
    }

    private void FillLineCharOffsets(TextLine line) {
        line.CharOffsets = ArrayPool<float>.Shared.Rent(line.CharCount + 1);

        var spacing = CharacterSpacing * TextScale;

        var x = 0f;
        var writeIndex = 0;

        for (var ti = line.StartToken; ti < line.EndToken; ti++) {
            var token = _tokens[ti];

            if (token.Kind != MarkupTokenKind.Text &&
                token.Kind != MarkupTokenKind.Whitespace)
                continue;

            var start = Math.Max(token.StartIndex, line.StartIndex);
            var end = Math.Min(token.EndIndex, line.EndIndex);

            for (var ci = start; ci < end; ci++) {
                var offset = ci - token.StartIndex;

                line.CharOffsets[writeIndex++] = x;

                x += token.CharacterAdvances[offset];

                if (token.Kind == MarkupTokenKind.Text && ci < end - 1) {
                    x += spacing;
                }
            }
        }

        line.CharOffsets[line.CharCount] = x;
    }

    private void CalculateTokenRenderSize(MarkupToken token) {
        var renderWidth = 0f;
        var characterSpacing = CharacterSpacing * TextScale;

        var length = token.Length;
        token.CharacterAdvances = ArrayPool<float>.Shared.Rent(length);
        token.CharAdvanceCount = length;

        for (var i = 0; i < length; i++) {
            var c = token[i];

            float advance;

            if (c == '\t') {
                advance = TabSize;
            }
            else {
                Font.GetCharacterInfo(c, out var info, TextScale);
                advance = info.advance;
                if (c == ' ') {
                    advance += SpaceAdditionalWidth;
                }
            }

            token.CharacterAdvances[i] = advance;

            renderWidth += advance;

            if (token.Kind == MarkupTokenKind.Text && i < length - 1)
                renderWidth += characterSpacing;
        }

        token.RenderHeight = Font.Baseline * TextScale;
        token.RenderWidth = renderWidth;
    }

    public static DynamicFontRenderer Rent(DynamicFont dynamicFont) {
        var instance = Pool.Count > 0 ? Pool.Pop() : new DynamicFontRenderer();
        instance.Font = dynamicFont;
        return instance;
    }
}