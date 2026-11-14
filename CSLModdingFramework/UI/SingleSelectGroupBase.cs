using System;
using System.Collections.Generic;

namespace CSLModdingFramework.UI;

public abstract class SingleSelectGroupBase<T, TOption> : OptionsGroupBase<T, TOption> where TOption : SelectableOptionBase<T> {
    protected TOption _selectedOption;

    public event Action<TOption> OptionSelectionChanged;
    public event Action<T> ValueSelectionChanged;
    public event Action<SelectableItem> SelectableItemCreated;

    public TOption SelectionOption => _selectedOption;

    protected virtual void OnOptionSelectionChanged(TOption option, bool notify = true) {
        if (!notify) return;

        NotifyOptionSelectionChanged(option);
        NotifyValueSelectionChanged(option.Value);
    }

    public bool SetSelected(Func<TOption, bool> predicate, bool notify = true) {
        if (predicate == null) return false;

        foreach (var option in _options) {
            if (predicate(option))
                return SetSelected(option, notify);
        }

        return false;
    }

    public bool SetSelected(int id, bool notify = true) => _optionsById.TryGetValue(id, out var option) && SetSelected(option, notify);

    public bool SetSelected(T value, bool notify = true) {
        foreach (var option in _options) {
            if (EqualityComparer<T>.Default.Equals(option.Value, value)) {
                return SetSelected(option, notify);
            }
        }

        return false;
    }

    public bool SetSelected(TOption option, bool notify = true) {
        if (option is null
            || !_optionsById.TryGetValue(option.Id, out var existingOption)
            || !ReferenceEquals(existingOption, option)) return false;

        if (ReferenceEquals(_selectedOption, existingOption)) return false;

        var oldSelected = _selectedOption;

        if (oldSelected != null) {
            oldSelected.IsSelected = false;
            OnOptionSelectionChanged(oldSelected, false);
        }

        existingOption.IsSelected = true;
        _selectedOption = existingOption;

        OnOptionSelectionChanged(existingOption, notify);
        return true;
    }

    public bool SetSelectedWithoutNotify(Func<TOption, bool> predicate) => SetSelected(predicate, false);
    public bool SetSelectedWithoutNotify(int id) => SetSelected(id, false);
    public bool SetSelectedWithoutNotify(T value) => SetSelected(value, false);
    public bool SetSelectedWithoutNotify(TOption option) => SetSelected(option, false);

    protected void NotifyOptionSelectionChanged(TOption option) => OptionSelectionChanged?.Invoke(option);
    protected void NotifyValueSelectionChanged(T value) => ValueSelectionChanged?.Invoke(value);
    protected void NotifySelectableItemCreated(SelectableItem item) => SelectableItemCreated?.Invoke(item);
}