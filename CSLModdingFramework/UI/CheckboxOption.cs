using System;

namespace CSLModdingFramework.UI;

public class CheckboxOption<T> : CheckableOption<T, CheckboxOption<T>> {
    public CheckboxOption(T value) : base(value) { }

    public CheckboxOption(T value, string header = null, string description = null, bool isChecked = false, bool isEnabled = true, bool isVisible = true, Action<CheckboxOption<T>> checkedCallback = null) : base(value, isChecked, isEnabled, isVisible, checkedCallback) {
        Header = header;
        Description = description;
    }

    internal CheckboxOption(int id, T value) : base(id, value) { }

    public string Description { get; internal set; }
    public string Header { get; internal set; }
}