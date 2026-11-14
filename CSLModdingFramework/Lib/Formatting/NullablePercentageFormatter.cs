using CSLModdingFramework.Extensions;

namespace CSLModdingFramework.Lib.Formatting;

public sealed class NullablePercentageFormatter : IValueFormatter<double?> {
    private readonly PercentageFormatter _inner;

    public string FormatString {
        get => _inner.FormatString;
        set => _inner.FormatString = value;
    }

    public NullablePercentageFormatter(PercentageFormatter inner) {
        _inner = inner;
    }

    public string Format(double? value) => !value.HasValue ? "" : _inner.Format(value.Value);

    public bool TryParse(string text, out double? value) {
        if (text.IsNullOrWhiteSpace()) {
            value = null;
            return true;
        }

        if (_inner.TryParse(text, out var parsed)) {
            value = parsed;
            return true;
        }

        value = null;
        return false;
    }
}