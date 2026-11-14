using System;

namespace CSLModdingFramework.UI;

public abstract class CheckableOption<T, TOption> : OptionBase<T> where TOption:CheckableOption<T, TOption>{
    public CheckableOption(T value) : base(value) { }

    public CheckableOption(T value, bool isChecked = false, bool isEnabled = true, bool isVisible = true, Action<TOption> checkedCallback = null) : base(value, isEnabled, isVisible) {
        IsChecked = isChecked;
        CheckedCallback = checkedCallback;
    }

    internal CheckableOption(int id, T value) : base(id, value) { }
    
    public bool IsChecked { get; internal set; }
    internal Action<TOption> CheckedCallback { get; set; }
}