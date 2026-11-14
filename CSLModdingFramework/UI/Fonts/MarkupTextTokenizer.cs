using System.Collections.Generic;
using UnityEngine;

namespace CSLModdingFramework.UI.Fonts;

public static class MarkupTextTokenizer {
    public static void Tokenize(string source, List<MarkupToken> tokens) {
        if (string.IsNullOrEmpty(source)) {
            return;
        }

        var index = 0;
        var length = source.Length;

        while (index < length) {
            var c = source[index];

            if (c == '<') {
                if (ReadColor(tokens, source, ref index)) {
                    continue;
                }
            }

            if (c is '\r' or '\n') {
                ReadNewline(tokens, source, ref index);
            }
            else if (char.IsWhiteSpace(c)) {
                ReadWhitespace(tokens, source, ref index);
            }
            else {
                ReadText(tokens, source, ref index);
            }
        }
    }

    private static int GetHex(char c) {
        if (c is >= '0' and <= '9') {
            return c - '0';
        }

        if (c is >= 'a' and <= 'f') {
            return c - 'a' + 10;
        }

        if (c is >= 'A' and <= 'F') {
            return c - 'A' + 10;
        }

        return -1;
    }

    private static bool IsMarkupStart(string source, int index) => Match(source, index, "<color=") || Match(source, index, "</color>");

    private static bool Match(string source, int index, string value) {
        if (index + value.Length > source.Length) {
            return false;
        }

        for (var i = 0; i < value.Length; i++) {
            if (source[index + i] != value[i]) {
                return false;
            }
        }

        return true;
    }

    private static bool ReadColor(List<MarkupToken> tokens, string source, ref int index) {
        var start = index;

        const string endTag = "</color>";
        if (Match(source, index, endTag)) {
            index += endTag.Length;
            tokens.Add(MarkupToken.Rent(MarkupTokenKind.ColorEnd, source, start, index));
            return true;
        }

        const string startTag = "<color=";
        if (!Match(source, index, startTag)) {
            return false;
        }

        index += startTag.Length;
        var colorStart = index;

        while (index < source.Length && source[index] != '>') {
            index++;
        }

        if (index >= source.Length) {
            tokens.Add(MarkupToken.Rent(MarkupTokenKind.Text, source, start, index));
            return true;
        }

        var colorLength = index - colorStart;
        if (!TryParseColor(source, colorStart, colorLength, out var color)) {
            tokens.Add(MarkupToken.Rent(MarkupTokenKind.Text, source, start, index + 1));
            index++;
            return true;
        }

        index++;
        var token = MarkupToken.Rent(MarkupTokenKind.ColorStart, source, start, index);
        token.Color = color;
        tokens.Add(token);

        return true;
    }

    private static void ReadNewline(List<MarkupToken> tokens, string source, ref int index) {
        var start = index;

        if (source[index] == '\r' &&
            index + 1 < source.Length &&
            source[index + 1] == '\n') {
            index += 2;
        }
        else {
            index++;
        }

        tokens.Add(MarkupToken.Rent(MarkupTokenKind.Newline, source, start, index));
    }

    private static void ReadText(List<MarkupToken> tokens, string source, ref int index) {
        var start = index;

        while (index < source.Length) {
            var c = source[index];

            if (c == '\r' ||
                c == '\n' ||
                char.IsWhiteSpace(c)) {
                break;
            }


            if (c == '<' && IsMarkupStart(source, index)) {
                break;
            }


            index++;
        }

        tokens.Add(MarkupToken.Rent(MarkupTokenKind.Text, source, start, index));
    }

    private static void ReadWhitespace(List<MarkupToken> tokens, string source, ref int index) {
        var start = index;

        while (index < source.Length) {
            var c = source[index];

            if (c is '\r' or '\n') {
                break;
            }

            if (!char.IsWhiteSpace(c)) {
                break;
            }

            index++;
        }

        tokens.Add(MarkupToken.Rent(MarkupTokenKind.Whitespace, source, start, index));
    }

    private static bool TryHex(string source, int index, out byte value) {
        value = 0;
        if (index + 1 >= source.Length) {
            value = 0;
            return false;
        }

        var high = GetHex(source[index]);
        var low = GetHex(source[index + 1]);

        if (high < 0 || low < 0) {
            return false;
        }

        value = (byte)((high << 4) | low);

        return true;
    }

    private static bool TryParseColor(string source, int start, int length, out Color32 color) {
        color = default;

        if (length != 7 && length != 9) {
            return false;
        }

        if (source[start] != '#') {
            return false;
        }

        if (!TryHex(source, start + 1, out var r) ||
            !TryHex(source, start + 3, out var g) ||
            !TryHex(source, start + 5, out var b)) {
            return false;
        }

        byte a = 255;

        if (length == 9 &&
            !TryHex(source, start + 7, out a)) {
            return false;
        }

        color = new Color32(r, g, b, a);

        return true;
    }
}