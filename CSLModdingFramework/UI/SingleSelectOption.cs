namespace CSLModdingFramework.UI;

public class SingleSelectOption<T> : SelectableOptionBase<T> {
    public SingleSelectOption(T value, string displayText = null, bool isEnabled = true, bool isVisible = true, IAtlas iconAtlas = null, string iconName = null) : base(value, isEnabled, isVisible) {
        DisplayText = displayText;
        IconAtlas = iconAtlas;
        IconName = iconName;
    }

    internal SingleSelectOption(int id, T value) : base(id, value) { }

    public string DisplayText { get; internal set; }
    public IAtlas IconAtlas { get; internal set; }
    public string IconName { get; internal set; }
}