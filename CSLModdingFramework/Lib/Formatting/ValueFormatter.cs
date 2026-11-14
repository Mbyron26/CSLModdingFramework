namespace CSLModdingFramework.Lib.Formatting;

public abstract class ValueFormatter<T> : IValueFormatter<T> {
    private string _formatString;

    public string FormatString {
        get => _formatString;
        set => SetFormatString(value);
    }

    public event FormatChangedEventHandler FormatChanged;

    protected ValueFormatter(string formatString) => _formatString = formatString ?? "G";

    public abstract string Format(T value);
    public abstract bool TryParse(string text, out T value);

    protected virtual void OnFormatChanged(string oldValue, string newValue) {
        FormatChanged?.Invoke(oldValue, newValue);
    }

    public bool SetFormatString(string newFormat, bool notify = true) {
        if (_formatString == newFormat) return false;

        var old = _formatString;
        _formatString = newFormat;
        
        if (notify)
            OnFormatChanged(old, _formatString);
        return true;
    }
}