using System;

namespace CSLModdingFramework.Localization;

public sealed class LocalizationResourceInfo {
    public string EnglishName { get; set; }
    public string RawLanguageCode { get; set; }
    public string NormalizedLanguageCode { get; set; }
    public int TotalStrings { get; set; }
    public int TranslatedStrings { get; set; }

    public override string ToString() => $"{EnglishName}, {RawLanguageCode}({NormalizedLanguageCode}), {TranslatedStrings}/{TotalStrings}";

    internal void MergeFrom(LocalizationResourceInfo other) {
        if (other is null) return;

        TotalStrings += other.TotalStrings;
        TranslatedStrings += other.TranslatedStrings;
    }

    internal void AddStrings(int count) {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        TotalStrings += count;
    }

    internal void IncrementTranslated(int count = 1) {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        TranslatedStrings += count;
        if (TranslatedStrings > TotalStrings) TranslatedStrings = TotalStrings;
    }
}