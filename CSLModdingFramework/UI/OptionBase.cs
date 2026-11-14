namespace CSLModdingFramework.UI;

public class OptionBase<T> {
    public OptionBase(T value) => Value = value;

    internal OptionBase(int id, T value) : this(value) => Id = id;

    protected OptionBase(T value, bool isEnabled, bool isVisible) : this(value) {
        IsEnabled = isEnabled;
        IsVisible = isVisible;
    }

    public int Id { get; internal set; }
    public bool IsEnabled { get; internal set; } = true;
    public bool IsVisible { get; internal set; } = true;
    public T Value { get; internal set;}
}