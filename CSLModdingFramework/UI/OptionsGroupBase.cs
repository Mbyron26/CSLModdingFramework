using System;
using System.Collections.Generic;
using System.Linq;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;

namespace CSLModdingFramework.UI;

public abstract class OptionsGroupBase<T, TOption> where TOption : OptionBase<T> {
    protected readonly List<TOption> _options = new();
    protected readonly Dictionary<int, TOption> _optionsById = new();
    protected int _nextId;
    protected IReadOnlyList<TOption> _readOnlyOptions;
    protected IReadOnlyDictionary<int, TOption> _readOnlyOptionsById;

    public Func<T, bool> DefaultEnabledSelector { get; set; }
    public Func<T, bool> DefaultVisibleSelector { get; set; }
    public IReadOnlyList<TOption> Options => _readOnlyOptions ??= _options.AsReadOnlyList();
    public IReadOnlyDictionary<int, TOption> OptionsById => _readOnlyOptionsById ??= _optionsById.AsReadOnlyDictionary();
    public int OptionsCount => _options.Count;

    protected abstract TOption CreateOption(int id, T value);
    protected abstract void InitializeOption(TOption option);

    public virtual IList<TOption> AddOptions(IEnumerable<TOption> options) {
        if (options == null) throw new ArgumentNullException(nameof(options));

        var addedOptions = new List<TOption>();

        foreach (var option in options) {
            if (option == null) throw new ArgumentException("The collection contains a null option.", nameof(options));

            if (_options.Contains(option)) throw new ArgumentException("The option has already been added.", nameof(options));

            InitializeOption(option);

            option.Id = _nextId++;
            _options.Add(option);
            _optionsById.Add(option.Id, option);
            addedOptions.Add(option);
        }

        if (addedOptions.Count > 0)
            OnOptionsAdded(addedOptions);

        return addedOptions;
    }

    protected abstract void OnOptionAdded(TOption option);
    protected abstract void OnOptionEnabledChanged(TOption option);
    protected abstract void OnOptionRemoved(TOption option);
    protected abstract void OnOptionVisibleChanged(TOption option);
    protected abstract void OnOptionsAdded(List<TOption> options);
    protected abstract void OnOptionsEnabledChanged(List<TOption> options);
    protected abstract void OnOptionsRemoved(List<TOption> removedOptions);
    protected abstract void OnOptionsSorted();
    protected abstract void OnOptionsVisibleChanged(List<TOption> changedOptions);

    public TOption AddOption(TOption option) {
        if (option == null) throw new ArgumentNullException(nameof(option));

        if (_options.Contains(option)) throw new ArgumentException("The option has already been added.", nameof(option));

        InitializeOption(option);

        option.Id = _nextId++;
        _options.Add(option);
        _optionsById.Add(option.Id, option);
        OnOptionAdded(option);

        return option;
    }

    public IList<TOption> AddOptions(IEnumerable<T> values) {
        if (values == null) throw new ArgumentNullException(nameof(values));

        var addedOptions = new List<TOption>();
        foreach (var value in values) {
            var option = CreateOption(_nextId++, value);

            _options.Add(option);
            _optionsById.Add(option.Id, option);
            addedOptions.Add(option);
        }

        if (addedOptions.Count > 0)
            OnOptionsAdded(addedOptions);

        return addedOptions;
    }

    public IList<TOption> AddOptions(params T[] values) => values == null
        ? throw new ArgumentNullException(nameof(values))
        : AddOptions((IEnumerable<T>)values);

    public IList<TOption> AddOptions(params TOption[] options) => options == null
        ? throw new ArgumentNullException(nameof(options))
        : AddOptions((IEnumerable<TOption>)options);

    public void ClearOptions() {
        if (_options.Count == 0) return;

        var removedOptions = new List<TOption>(_options);

        _options.Clear();
        _optionsById.Clear();

        OnOptionsRemoved(removedOptions);
    }

    public TOption GetOption(Func<TOption, bool> predicate) {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        for (var i = 0; i < _options.Count; i++) {
            var option = _options[i];

            if (predicate(option))
                return option;
        }

        return null;
    }

    public TOption GetOption(T value) {
        for (var i = 0; i < _options.Count; i++) {
            var option = _options[i];

            if (EqualityComparer<T>.Default.Equals(option.Value, value))
                return option;
        }

        return null;
    }

    public IEnumerable<TOption> GetOptions(Func<TOption, bool> predicate) {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        for (var i = 0; i < _options.Count; i++) {
            var option = _options[i];

            if (predicate(option))
                yield return option;
        }
    }

    public bool RemoveOption(T value) {
        for (var i = 0; i < _options.Count; i++) {
            if (EqualityComparer<T>.Default.Equals(_options[i].Value, value))
                return RemoveOption(_options[i]);
        }

        return false;
    }

    public bool RemoveOption(TOption option) {
        if (option == null)
            throw new ArgumentNullException(nameof(option));

        if (!_optionsById.TryGetValue(option.Id, out var existingOption) || !ReferenceEquals(existingOption, option))
            return false;

        _optionsById.Remove(option.Id);
        _options.Remove(existingOption);

        OnOptionRemoved(existingOption);
        return true;
    }

    public int RemoveOptions(IEnumerable<TOption> options) {
        if (options == null) throw new ArgumentNullException(nameof(options));

        var toRemove = options.ToList();
        var removedOptions = new List<TOption>();

        foreach (var option in toRemove) {
            if (option == null) throw new ArgumentException("The collection contains a null option.", nameof(options));

            if (!_optionsById.TryGetValue(option.Id, out var existingOption) || !ReferenceEquals(existingOption, option))
                continue;

            _optionsById.Remove(option.Id);
            _options.Remove(existingOption);
            removedOptions.Add(existingOption);
        }

        if (removedOptions.Count > 0)
            OnOptionsRemoved(removedOptions);

        return removedOptions.Count;
    }

    public int RemoveOptions(params T[] values) => values == null ? throw new ArgumentNullException(nameof(values)) : RemoveOptions((IEnumerable<T>)values);

    public int RemoveOptions(IEnumerable<T> values) {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var toRemove = values.ToList();
        var removedOptions = new List<TOption>();

        foreach (var value in toRemove) {
            var option = GetOption(value);

            if (option == null) continue;

            if (!_optionsById.TryGetValue(option.Id, out var existingOption) || !ReferenceEquals(existingOption, option))
                continue;

            _optionsById.Remove(option.Id);
            _options.Remove(existingOption);
            removedOptions.Add(existingOption);
        }

        if (removedOptions.Count > 0)
            OnOptionsRemoved(removedOptions);

        return removedOptions.Count;
    }

    public int RemoveOptions(params TOption[] options) => options == null ? throw new ArgumentNullException(nameof(options)) : RemoveOptions((IEnumerable<TOption>)options);

    public int SetAllEnabled(bool isEnabled) {
        var changedOptions = new List<TOption>();

        foreach (var option in _options) {
            if (option.IsEnabled == isEnabled) continue;

            option.IsEnabled = isEnabled;
            changedOptions.Add(option);
        }

        if (changedOptions.Count > 0)
            OnOptionsEnabledChanged(changedOptions);

        return changedOptions.Count;
    }

    public bool SetEnabled(TOption option, bool isEnabled) {
        if (option == null || !_optionsById.TryGetValue(option.Id, out var existingOption) || !ReferenceEquals(existingOption, option) || existingOption.IsEnabled == isEnabled)
            return false;

        existingOption.IsEnabled = isEnabled;
        OnOptionEnabledChanged(existingOption);
        return true;
    }

    public bool SetEnabled(int id, bool isEnabled) => _optionsById.TryGetValue(id, out var option) && SetEnabled(option, isEnabled);

    public int SetEnabled(Func<TOption, bool> predicate, bool isEnabled) {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var changedOptions = new List<TOption>();

        foreach (var option in _options) {
            if (!predicate(option) || option.IsEnabled == isEnabled)
                continue;

            option.IsEnabled = isEnabled;
            changedOptions.Add(option);
        }

        if (changedOptions.Count > 0)
            OnOptionsEnabledChanged(changedOptions);

        return changedOptions.Count;
    }

    public int SetEnabled(IEnumerable<TOption> options, bool isEnabled) {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var changedOptions = new List<TOption>();

        foreach (var option in options) {
            if (option == null)
                throw new ArgumentException("The collection contains a null option.", nameof(options));

            if (!_optionsById.TryGetValue(option.Id, out var existingOption) || !ReferenceEquals(existingOption, option) || existingOption.IsEnabled == isEnabled)
                continue;

            existingOption.IsEnabled = isEnabled;
            changedOptions.Add(existingOption);
        }

        if (changedOptions.Count > 0)
            OnOptionsEnabledChanged(changedOptions);

        return changedOptions.Count;
    }

    public int SetVisible(Func<TOption, bool> predicate, bool isVisible) {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var changedOptions = new List<TOption>();

        foreach (var option in _options) {
            if (!predicate(option) || option.IsVisible == isVisible)
                continue;

            option.IsVisible = isVisible;
            changedOptions.Add(option);
        }

        if (changedOptions.Count > 0)
            OnOptionsVisibleChanged(changedOptions);

        return changedOptions.Count;
    }

    public bool SetVisible(T value, bool isVisible) {
        var option = GetOption(value);
        return option != null && SetVisible(option, isVisible);
    }

    public int SetVisible(IEnumerable<T> values, bool isVisible) {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var options = new List<TOption>();

        foreach (var value in values) {
            var option = GetOption(value);
            if (option != null)
                options.Add(option);
        }

        return options.Count > 0 ? SetVisible(options, isVisible) : 0;
    }

    public bool SetVisible(TOption option, bool isVisible) {
        if (option == null
            || !_optionsById.TryGetValue(option.Id, out var existingOption)
            || !ReferenceEquals(existingOption, option)
            || existingOption.IsVisible == isVisible)
            return false;

        existingOption.IsVisible = isVisible;
        OnOptionVisibleChanged(existingOption);
        return true;
    }

    public int SetVisible(IEnumerable<TOption> options, bool isVisible) {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var changedOptions = new List<TOption>();

        foreach (var option in options) {
            if (option == null)
                throw new ArgumentException("The collection contains a null option.", nameof(options));

            if (!_optionsById.TryGetValue(option.Id, out var existingOption)
                || !ReferenceEquals(existingOption, option)
                || existingOption.IsVisible == isVisible)
                continue;

            existingOption.IsVisible = isVisible;
            changedOptions.Add(existingOption);
        }

        if (changedOptions.Count > 0)
            OnOptionsVisibleChanged(changedOptions);

        return changedOptions.Count;
    }

    public void Sort(Comparison<TOption> comparison) {
        if (comparison == null)
            throw new ArgumentNullException(nameof(comparison));

        _options.Sort(comparison);
        OnOptionsSorted();
    }
}