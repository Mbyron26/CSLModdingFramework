using System.Collections.Generic;
using CSLModdingFramework.UI.Utilities;

namespace CSLModdingFramework.UI.Fonts;

public static class PlainTextTokenizer {
    public static void Tokenize(string source, List<MarkupToken> tokens) {
        if (string.IsNullOrEmpty(source)) {
            return;
        }

        var index = 0;
        var length = source.Length;

        while (index < length) {
            var c = source[index];

            if (c is '\r' or '\n') {
                ReadNewline(tokens, source, ref index);
                continue;
            }

            if (char.IsWhiteSpace(c)) {
                ReadWhitespace(tokens, source, ref index);
                continue;
            }

            ReadText(tokens, source, ref index);
        }
    }
    
    /*
     * Splits CJK characters into individual tokens while keeping Latin words as grouped tokens. Enables natural character-level wrapping for CJK text and word-level wrapping for Latin text.
     */
    // private static void ReadText(List<MarkupToken> tokens, string source, ref int index) {
    //     var start = index;
    //     var length = source.Length;
    //
    //     if (TextUtilities.IsCjk(source[index])) {
    //         index++;
    //
    //         tokens.Add(MarkupToken.Rent(
    //             MarkupTokenKind.Text,
    //             source,
    //             start,
    //             index
    //         ));
    //
    //         return;
    //     }
    //
    //     while (index < length) {
    //         var c = source[index];
    //
    //         if (c is '\r' or '\n')
    //             break;
    //
    //         if (char.IsWhiteSpace(c))
    //             break;
    //
    //         if (TextUtilities.IsCjk(c))
    //             break;
    //
    //         index++;
    //     }
    //
    //     tokens.Add(MarkupToken.Rent(
    //         MarkupTokenKind.Text,
    //         source,
    //         start,
    //         index
    //     ));
    // }
    
    /*
     * Splits text only by whitespace and newline characters. Non-whitespace text remains as a single token.
     */
    private static void ReadText(List<MarkupToken> tokens, string source, ref int index) {
        var start = index;
        var length = source.Length;

        while (index < length) {
            var c = source[index];

            if (c is '\r' or '\n')
                break;

            if (char.IsWhiteSpace(c))
                break;

            index++;
        }

        tokens.Add(MarkupToken.Rent(MarkupTokenKind.Text, source, start, index));
    }

    private static void ReadWhitespace(List<MarkupToken> tokens, string source, ref int index) {
        var start = index;
        var length = source.Length;

        while (index < length) {
            var c = source[index];

            if (c is '\r' or '\n')
                break;

            if (!char.IsWhiteSpace(c))
                break;

            index++;
        }

        tokens.Add(MarkupToken.Rent(MarkupTokenKind.Whitespace, source, start, index));
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
}