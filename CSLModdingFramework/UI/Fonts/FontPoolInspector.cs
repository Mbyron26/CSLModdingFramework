using System.Collections.Generic;

namespace CSLModdingFramework.UI.Fonts;

public class FontPoolInspector {
    public static int DynamicFontRendererPoolCount => DynamicFontRendererPool.Count;
    public static int MarkupTokenPoolCount => MarkupTokenPool.Count;
    public static int TextLinePoolCount => TextLinePool.Count;
    internal static Stack<DynamicFontRenderer> DynamicFontRendererPool => DynamicFontRenderer.Pool;
    internal static Stack<MarkupToken> MarkupTokenPool => MarkupToken.Pool;
    internal static Stack<TextLine> TextLinePool => TextLine.Pool;
}