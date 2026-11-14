using System;

namespace CSLModdingFramework.UI;

public abstract class RadioOptionsGroupBase<T> : SingleSelectGroupBase<T, RadioOption<T>> {
    public Func<T, string> DefaultDescriptionSelector { get; set; }
    public Func<T, string> DefaultHeaderSelector { get; set; }

    public virtual RadioOption<T> AddOption(T value, string header = null, string description = null, bool isEnabled = true, bool isVisible = true) {
        var id = _nextId++;
        var option = new RadioOption<T>(id, value) {
            Header = header ?? DefaultHeaderSelector?.Invoke(value) ?? (value != null ? value.ToString() : null),
            Description = description ?? DefaultDescriptionSelector?.Invoke(value),
            IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? isEnabled,
            IsVisible = DefaultVisibleSelector?.Invoke(value) ?? isVisible,
        };

        _options.Add(option);
        _optionsById.Add(id, option);
        OnOptionAdded(option);
        return option;
    }

    protected override RadioOption<T> CreateOption(int id, T value) {
        var option = new RadioOption<T>(id, value) {
            Header = DefaultHeaderSelector?.Invoke(value) ?? (value != null ? value.ToString() : null),
            Description = DefaultDescriptionSelector?.Invoke(value),
            IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? true,
            IsVisible = DefaultVisibleSelector?.Invoke(value) ?? true,
        };
        return option;
    }

    protected override void InitializeOption(RadioOption<T> option) {
        var value = option.Value;
        option.Header ??= DefaultHeaderSelector?.Invoke(value) ?? (value != null ? value.ToString() : null);
        option.Description ??= DefaultDescriptionSelector?.Invoke(value);
        option.IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? option.IsEnabled;
        option.IsVisible = DefaultVisibleSelector?.Invoke(value) ?? option.IsVisible;
    }
}