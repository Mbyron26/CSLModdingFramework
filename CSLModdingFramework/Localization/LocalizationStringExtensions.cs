using CSLModdingFramework.Core;

namespace CSLModdingFramework.Localization;

public static class LocalizationStringExtensions {
    public static FormattedString Format(this LocalizationString value, params object[] args) => new(value.Key, args);
}