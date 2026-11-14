namespace CSLModdingFramework.UI;

public class MultiSelectOption<T> : SingleSelectOption<T> {
    public MultiSelectOption(
        T value,
        bool isSelected = false,
        string displayText = null,
        bool isEnabled = true,
        bool isVisible = true,
        IAtlas iconAtlas = null,
        string iconName = null)
        : base(value, displayText, isEnabled, isVisible, iconAtlas, iconName) =>
        IsSelected = isSelected;

    internal MultiSelectOption(int id, T value) : base(id, value) { }
}