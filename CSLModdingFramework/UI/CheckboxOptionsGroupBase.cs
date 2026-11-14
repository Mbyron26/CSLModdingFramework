using System;
using System.Collections.Generic;
using System.Linq;

namespace CSLModdingFramework.UI;

public abstract class CheckboxOptionsGroupBase<T> : OptionsGroupBase<T, CheckboxOption<T>> {
    public event Action<CheckboxOption<T>, bool> OptionCheckedChanged;
    public event Action<IList<CheckboxOption<T>>> OptionsCheckedChanged;
    public event Action<CheckboxOptionsGroupBase<T>, CheckboxOption<T>> GroupOptionCheckedChanged;
    public event Action<CheckboxOptionsGroupBase<T>, IList<CheckboxOption<T>>> GroupOptionsCheckedChanged;

    public Func<T, Action<CheckboxOption<T>>> DefaultCheckedCallbackSelector { get; set; }
    public Func<T, bool> DefaultCheckedSelector { get; set; }
    public Func<T, string> DefaultDescriptionSelector { get; set; }
    public Func<T, string> DefaultHeaderSelector { get; set; }

    public virtual CheckboxOption<T> AddOption(T value, bool isChecked = false, string header = null, string description = null, bool isEnabled = true, bool isVisible = true, Action<CheckboxOption<T>> onCheckedChanged = null) {
        var id = _nextId++;
        var option = new CheckboxOption<T>(id, value) {
            Header = header ?? DefaultHeaderSelector?.Invoke(value) ?? (value != null ? value.ToString() : null),
            Description = description ?? DefaultDescriptionSelector?.Invoke(value),
            IsChecked = DefaultCheckedSelector?.Invoke(value) ?? isChecked,
            IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? isEnabled,
            IsVisible = DefaultVisibleSelector?.Invoke(value) ?? isVisible,
            CheckedCallback = onCheckedChanged ?? DefaultCheckedCallbackSelector?.Invoke(value)
        };

        _options.Add(option);
        _optionsById.Add(id, option);
        OnOptionAdded(option);
        return option;
    }

    protected virtual void OnOptionCheckedChanged(CheckboxOption<T> option, bool notify = true) {
        if (!notify) return;

        option.CheckedCallback?.Invoke(option);
        NotifyOptionCheckedChanged(option, option.IsChecked);
        NotifyGroupOptionCheckedChanged(this, option);
    }

    protected virtual void OnOptionsCheckedChanged(List<CheckboxOption<T>> options, bool notify = true) {
        if (!notify) return;

        foreach (var option in options) {
            option.CheckedCallback?.Invoke(option);
        }

        NotifyGroupOptionsCheckedChanged(this, options);
        NotifyOptionsCheckedChanged(options);
    }

    protected override CheckboxOption<T> CreateOption(int id, T value) {
        var option = new CheckboxOption<T>(id, value) {
            Header = DefaultHeaderSelector?.Invoke(value) ?? (value != null ? value.ToString() : null),
            Description = DefaultDescriptionSelector?.Invoke(value),
            IsChecked = DefaultCheckedSelector?.Invoke(value) ?? false,
            IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? true,
            IsVisible = DefaultVisibleSelector?.Invoke(value) ?? true,
            CheckedCallback = DefaultCheckedCallbackSelector?.Invoke(value)
        };
        return option;
    }

    protected override void InitializeOption(CheckboxOption<T> option) {
        var value = option.Value;
        option.Header ??= DefaultHeaderSelector?.Invoke(value) ?? (value != null ? value.ToString() : null);
        option.Description ??= DefaultDescriptionSelector?.Invoke(value);
        option.IsChecked = DefaultCheckedSelector?.Invoke(value) ?? option.IsChecked;
        option.IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? option.IsEnabled;
        option.IsVisible = DefaultVisibleSelector?.Invoke(value) ?? option.IsVisible;
        option.CheckedCallback ??= DefaultCheckedCallbackSelector?.Invoke(value);
    }

    public int CheckAll(bool notify = true) => SetAllChecked(true, notify);

    public IEnumerable<CheckboxOption<T>> GetCheckedOptions() => _options.Where(o => o.IsChecked);

    public IEnumerable<T> GetCheckedValues() => _options.Where(o => o.IsChecked).Select(o => o.Value);

    public CheckState GetSelectionState() {
        var allCount = 0;
        var checkedCount = 0;

        foreach (var option in _options) {
            if (!option.IsEnabled || !option.IsVisible)
                continue;

            allCount++;

            if (option.IsChecked)
                checkedCount++;
        }

        if (allCount == 0 || checkedCount == 0)
            return CheckState.Unchecked;

        if (checkedCount == allCount)
            return CheckState.Checked;

        return CheckState.Indeterminate;
    }

    public int SetAllChecked(bool isChecked, bool notify = true) {
        var changedOptions = new List<CheckboxOption<T>>();

        foreach (var option in _options) {
            if (option.IsChecked == isChecked)
                continue;

            option.IsChecked = isChecked;
            changedOptions.Add(option);
        }

        if (changedOptions.Count > 0)
            OnOptionsCheckedChanged(changedOptions, notify);

        return changedOptions.Count;
    }

    public bool SetChecked(CheckboxOption<T> option, bool isChecked, bool notify = true) {
        if (option is null || !_optionsById.TryGetValue(option.Id, out var existingOption) || !ReferenceEquals(existingOption, option) || existingOption.IsChecked == isChecked) return false;

        existingOption.IsChecked = isChecked;
        OnOptionCheckedChanged(existingOption, notify);

        return true;
    }

    public int SetChecked(IEnumerable<CheckboxOption<T>> options, bool isChecked, bool notify = true) {
        if (options == null) throw new ArgumentNullException(nameof(options));

        var changedOptions = new List<CheckboxOption<T>>();

        foreach (var option in options) {
            if (option is null) throw new ArgumentException("The collection contains a null option.", nameof(options));

            if (!_optionsById.TryGetValue(option.Id, out var existingOption) || !ReferenceEquals(existingOption, option) || existingOption.IsChecked == isChecked) continue;

            existingOption.IsChecked = isChecked;
            changedOptions.Add(existingOption);
        }

        if (changedOptions.Count > 0)
            OnOptionsCheckedChanged(changedOptions, notify);

        return changedOptions.Count;
    }

    public int SetChecked(Func<CheckboxOption<T>, bool> predicate, bool isChecked, bool notify = true) {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var changedOptions = new List<CheckboxOption<T>>();
        foreach (var option in _options) {
            if (!predicate(option) || option.IsChecked == isChecked)
                continue;

            option.IsChecked = isChecked;
            changedOptions.Add(option);
        }

        if (changedOptions.Count > 0)
            OnOptionsCheckedChanged(changedOptions, notify);

        return changedOptions.Count;
    }

    public bool SetChecked(int id, bool isChecked, bool notify = true) => _optionsById.TryGetValue(id, out var option) && SetChecked(option, isChecked, notify);

    public void ToggleAll() {
        if (GetSelectionState() == CheckState.Checked)
            UncheckAll();
        else
            CheckAll();
    }

    public int UncheckAll(bool notify = true) => SetAllChecked(false, notify);

    protected (int total, int checkedCount) GetSelectionCounts() {
        var allCount = 0;
        var checkCount = 0;

        foreach (var option in _options) {
            if (!option.IsEnabled || !option.IsVisible) {
                continue;
            }

            allCount++;

            if (option.IsChecked) {
                checkCount++;
            }
        }

        return (allCount, checkCount);
    }
    protected void NotifyGroupOptionCheckedChanged(CheckboxOptionsGroupBase<T> group, CheckboxOption<T> option) => GroupOptionCheckedChanged?.Invoke(group, option);
    protected void NotifyGroupOptionsCheckedChanged(CheckboxOptionsGroupBase<T> group, IList<CheckboxOption<T>> options) => GroupOptionsCheckedChanged?.Invoke(group, options);
    protected void NotifyOptionCheckedChanged(CheckboxOption<T> option, bool isChecked) => OptionCheckedChanged?.Invoke(option, isChecked);
    protected void NotifyOptionsCheckedChanged(IList<CheckboxOption<T>> options) => OptionsCheckedChanged?.Invoke(options);
}