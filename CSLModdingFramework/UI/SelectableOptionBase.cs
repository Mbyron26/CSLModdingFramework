namespace CSLModdingFramework.UI;

public abstract class SelectableOptionBase<T> : OptionBase<T> {
    protected SelectableOptionBase(T value) : base(value) { }

    protected SelectableOptionBase(int id, T value) : base(id, value) { }

    protected SelectableOptionBase(T value, bool isEnabled, bool isVisible) : base(value, isEnabled, isVisible) { }

    public bool IsSelected { get; internal set; }
}