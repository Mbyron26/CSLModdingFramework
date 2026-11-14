namespace CSLModdingFramework.UI;

public class RadioOption<T> : SelectableOptionBase<T> {
    public RadioOption(T value, string header = null, string description = null, bool isEnabled = true, bool isVisible = true) : base(value, isEnabled, isVisible) {
        Header = header;
        Description = description;
    }

    public RadioOption(T value) : base(value) { }

    internal RadioOption(int id, T value) : base(id, value) { }

    public string Description { get; internal set; }
    public string Header { get; internal set; }
}