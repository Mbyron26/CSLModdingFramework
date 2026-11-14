using System;

namespace CSLModdingFramework.Localization;

public readonly struct LocalizationString {
    public LocalizationString(string key, string defaultValue = null) {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        DefaultValue = defaultValue;
    }

    public string DefaultValue { get; }
    public string Key { get; }

    public override string ToString() => Key;

    public static implicit operator string(LocalizationString value) => value.Key;
}