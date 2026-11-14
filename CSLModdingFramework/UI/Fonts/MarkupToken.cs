using System;
using System.Collections.Generic;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;
using UnityEngine;

namespace CSLModdingFramework.UI.Fonts;

public sealed class MarkupToken {
    internal static readonly Stack<MarkupToken> Pool = new();

    public MarkupTokenKind Kind;
    public string Source;
    public int StartIndex;
    public int EndIndex;
    public float RenderWidth;
    public float RenderHeight;
    public float[] CharacterAdvances;
    public int CharAdvanceCount;
    public Color32 Color;

    public int Length => EndIndex - StartIndex;
    public string Value => Source.Substring(StartIndex, Length);

    private MarkupToken() { }

    public char this[int index] {
        get {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return Source[StartIndex + index];
        }
    }

    public void Reset() {
        Kind = MarkupTokenKind.Invalid;
        Source = null;
        StartIndex = 0;
        EndIndex = 0;
        RenderWidth = 0;
        RenderHeight = 0;
        if (CharacterAdvances != null) {
            ArrayPool<float>.Shared.Return(CharacterAdvances);
        }
        
        CharacterAdvances = ArrayExtensions.Empty<float>();
        CharAdvanceCount = 0;
        Color = default;
    }

    public static MarkupToken Rent(MarkupTokenKind kind, string source, int startIndex, int endIndex) {
        var token = Pool.Count > 0 ? Pool.Pop() : new MarkupToken();

        token.Kind = kind;
        token.Source = source;
        token.StartIndex = startIndex;
        token.EndIndex = endIndex;

        return token;
    }

    public void Return() {
        Reset();
        Pool.Push(this);
    }
}