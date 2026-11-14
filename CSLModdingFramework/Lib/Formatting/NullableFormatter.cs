using CSLModdingFramework.Extensions;

namespace CSLModdingFramework.Lib.Formatting;

public sealed class NullableFormatter<T> : IValueFormatter<T?> where T : struct {
    private readonly IValueFormatter<T> _inner;

    public string FormatString {
        get => _inner.FormatString;
        set => _inner.FormatString = value;
    }

    public NullableFormatter(IValueFormatter<T> inner) => _inner = inner;

    public string Format(T? value) => !value.HasValue ? "" : _inner.Format(value.Value);

    public bool TryParse(string text, out T? value) {
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