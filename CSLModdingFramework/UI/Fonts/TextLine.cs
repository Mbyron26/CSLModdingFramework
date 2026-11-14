using System;
using System.Collections.Generic;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;

namespace CSLModdingFramework.UI.Fonts;

public class TextLine {
    internal static readonly Stack<TextLine> Pool = new();

    public int StartIndex;
    public int EndIndex;
    public int StartToken;
    public int EndToken;
    public float Width;
    public float Height;
    public float[] CharWidths;
    public float[] CharOffsets;
    public int CharCount;

    private TextLine() { }

    public TextLine CopyFrom(TextLine other) {
        StartIndex = other.StartIndex;
        EndIndex = other.EndIndex;
        StartToken = other.StartToken;
        EndToken = other.EndToken;
        Width = other.Width;
        Height = other.Height;
        if (other.CharWidths != null) {
            CharWidths = ArrayPool<float>.Shared.Rent(other.CharCount);
            Array.Copy(other.CharWidths, CharWidths, other.CharCount);
            CharCount = other.CharCount;
        }
        else {
            CharWidths = ArrayExtensions.Empty<float>();
            CharCount = 0;
        }

        if (other.CharOffsets != null) {
            CharOffsets = ArrayPool<float>.Shared.Rent(other.CharCount + 1);
            Array.Copy(other.CharOffsets, CharOffsets, other.CharCount + 1);
        }
        else {
            CharOffsets = ArrayExtensions.Empty<float>();
        }

        return this;
    }

    public void Return() {
        Reset();
        Pool.Push(this);
    }

    public void Reset() {
        StartIndex = 0;
        EndIndex = 0;
        StartToken = 0;
        EndToken = 0;
        Width = 0;
        Height = 0;
        if (CharWidths != null) {
            ArrayPool<float>.Shared.Return(CharWidths);
        }

        if (CharOffsets != null) {
            ArrayPool<float>.Shared.Return(CharOffsets);
        }

        CharWidths = ArrayExtensions.Empty<float>();
        CharOffsets = ArrayExtensions.Empty<float>();
    }

    public static TextLine Rent() {
        var textLine = Pool.Count > 0 ? Pool.Pop() : new TextLine();
        return textLine;
    }

    public static TextLine Rent(int startIndex, int endIndex, int startToken, int endToken, float width, float height) {
        var textLine = Pool.Count > 0 ? Pool.Pop() : new TextLine();
        textLine.StartIndex = startIndex;
        textLine.EndIndex = endIndex;
        textLine.StartToken = startToken;
        textLine.EndToken = endToken;
        textLine.Width = width;
        textLine.Height = height;
        return textLine;
    }
}