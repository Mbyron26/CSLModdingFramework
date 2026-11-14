namespace CSLModdingFramework.Extensions;

public static class StringExtensions {
    public static bool IsNullOrWhiteSpace(this string value) {
        if (value == null) return true;
        for (var i = 0; i < value.Length; i++) {
            if (!char.IsWhiteSpace(value[i])) return false;
        }

        return true;
    }

    public static string SafeTrim(this string str) => str == null ? string.Empty : str.Trim();

    public static string SafeToLower(this string str) => str == null ? string.Empty : str.ToLower();

    public static string SafeToUpper(this string str) => str == null ? string.Empty : str.ToUpper();
}