using System;
using CSLModdingFramework.Extensions;

namespace CSLModdingFramework.Core;

public readonly struct FormattedString {
    public static readonly FormattedString Empty = new(string.Empty);

    public string Key { get; }
    public object[] Args { get; }

    public FormattedString(string key) {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Args = ArrayExtensions.Empty<object>();
    }

    public FormattedString(string key, params object[] args) {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Args = args ?? ArrayExtensions.Empty<object>();
    }

    public override string ToString() => Key;

    public string Resolve(Func<string, object[], string> formatter) => formatter == null ? throw new ArgumentNullException(nameof(formatter)) : formatter(Key, Args);

    public FormattedString WithParams(params object[] extraArgs) {
        if (extraArgs == null || extraArgs.Length == 0) return this;

        var combined = new object[Args.Length + extraArgs.Length];

        Args.CopyTo(combined, 0);
        extraArgs.CopyTo(combined, Args.Length);

        return new FormattedString(Key, combined);
    }

    public static implicit operator string(FormattedString value) => value.Key;

    public static implicit operator FormattedString(string key) => new(key);
}