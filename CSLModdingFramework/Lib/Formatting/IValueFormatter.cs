namespace CSLModdingFramework.Lib.Formatting;

public interface IValueFormatter<T> {
    string FormatString { get; set; }
    string Format(T value);
    bool TryParse(string text, out T value);
}