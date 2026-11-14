namespace CSLModdingFramework.UI.Utilities;

public static class TextUtilities {
    public static bool IsCjk(char c) => c is
        >= '\u4E00' and <= '\u9FFF' or
        >= '\u3400' and <= '\u4DBF' or
        >= '\u3040' and <= '\u30FF' or
        >= '\uAC00' and <= '\uD7AF' or
        >= '\u3000' and <= '\u303F'; 
}