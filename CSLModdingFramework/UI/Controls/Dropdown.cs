using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class Dropdown<T> : SingleSelectGroupBase<T, SelectableOption<T>> {
    private readonly List<SelectableOption<T>> _canWheelOptions = new();
    private readonly Dictionary<int, int> _canWheelOptionsIndexById = new();

    public event UIEventHandler<DropdownTriggerButton, SelectableListBox> PopupCreated;
    public event Func<SelectableOption<T>, string> TriggerButtonTextUpdated;

    public bool CanWheel { get; set; }
    public bool ClosePopupAfterSelected { get; set; } = true;
    public Func<T, IAtlas> DefaultIconAtlasSelector { get; set; }
    public Func<T, string> DefaultIconNameSelector { get; set; }
    public Func<T, string> DefaultTextSelector { get; set; }
    public SelectableListBox ListBox => TriggerButton == null ? null : TriggerButton.Popup;
    public DropdownTriggerButton TriggerButton { get; private set; }

    protected override SelectableOption<T> CreateOption(int id, T value) {
        var option = new SelectableOption<T>(id, value) {
            Text = DefaultTextSelector?.Invoke(value) ?? (value != null ? value.ToString() : null),
            IconName = DefaultIconNameSelector?.Invoke(value),
            IconAtlas = DefaultIconAtlasSelector?.Invoke(value),
            IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? true,
            IsVisible = DefaultVisibleSelector?.Invoke(value) ?? true,
        };

        return option;
    }

    protected override void InitializeOption(SelectableOption<T> option) {
        var value = option.Value;
        option.Text ??= DefaultTextSelector?.Invoke(value) ?? (value != null ? value.ToString() : null);
        option.IconName ??= DefaultIconNameSelector?.Invoke(value);
        option.IconAtlas ??= DefaultIconAtlasSelector?.Invoke(value);
        option.IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? option.IsEnabled;
        option.IsVisible = DefaultVisibleSelector?.Invoke(value) ?? option.IsVisible;
    }

    protected override void OnOptionAdded(SelectableOption<T> option) {
        AddCanWheelOption(option);
        AddSelectableItem(option);
    }

    protected override void OnOptionEnabledChanged(SelectableOption<T> option) {
        if (ListBox != null && ListBox.ItemsById.TryGetValue(option.Id, out var selectableItem)) {
            selectableItem.isEnabled = option.IsEnabled;
        }
    }

    protected override void OnOptionRemoved(SelectableOption<T> option) {
        if (ListBox != null && ListBox.ItemsById.TryGetValue(option.Id, out var selectableItem)) {
            selectableItem.eventClicked -= OnSelectableItemClicked;
            ListBox.DestroyItem(selectableItem.Id);
            ListBox.RefreshContentLayout();
        }
    }

    protected override void OnOptionSelectionChanged(SelectableOption<T> option, bool notify = true) {
        var text = option.Text;
        var id = option.Id;
        var isSelected = option.IsSelected;
        if (TriggerButton != null) {
            TriggerButton.SetTextWithoutNotify(TriggerButtonTextUpdated != null ? TriggerButtonTextUpdated(option) : text);
            var listBox = TriggerButton.Popup;
            if (listBox != null) {
                if (listBox.TryGetItem(id, out var item)) {
                    item.SetSelectionWithoutNotify(isSelected);
                }
            }
        }

        base.OnOptionSelectionChanged(option, notify);
    }

    protected override void OnOptionVisibleChanged(SelectableOption<T> option) {
        if (ListBox != null && ListBox.ItemsById.TryGetValue(option.Id, out var selectableItem)) {
            selectableItem.isVisible = option.IsVisible;
        }

        UpdateCanWheelOption();
    }

    protected override void OnOptionsAdded(List<SelectableOption<T>> options) {
        var hasListBox = ListBox != null;
        if (hasListBox)
            ListBox.SuspendAutoLayout();

        foreach (var option in _options) {
            AddCanWheelOption(option);

            if (!hasListBox) continue;

            var item = ListBox.AddItem(option.Id);
            SetupSelectableItem(item, option);
            NotifySelectableItemCreated(item);
        }

        if (hasListBox) {
            ListBox.ResumeAutoLayout();
            ListBox.RefreshContentLayout();
        }
    }

    protected override void OnOptionsEnabledChanged(List<SelectableOption<T>> options) {
        foreach (var option in options) {
            OnOptionEnabledChanged(option);
        }
    }

    protected override void OnOptionsRemoved(List<SelectableOption<T>> removedOptions) {
        if (ListBox == null) return;

        ListBox.SuspendAutoLayout();

        foreach (var option in removedOptions) {
            ListBox.DestroyItem(option.Id, item => item.eventClicked -= OnSelectableItemClicked);
        }

        ListBox.RefreshContentLayout();
        ListBox.ResumeAutoLayout();
    }

    protected override void OnOptionsSorted() {
        OnOptionsRemoved(_options);
        OnOptionsAdded(_options);
    }

    protected override void OnOptionsVisibleChanged(List<SelectableOption<T>> changedOptions) {
        UpdateCanWheelOption();

        if (ListBox == null) return;

        foreach (var changedOption in changedOptions) {
            if (ListBox.ItemsById.TryGetValue(changedOption.Id, out var selectableItem)) {
                selectableItem.isVisible = changedOption.IsVisible;
            }
        }
    }

    public SelectableOption<T> AddOption(T value, string text = null, bool isEnabled = true, bool isVisible = true, string iconName = null, IAtlas iconAtlas = null) {
        var id = _nextId++;
        var option = new SelectableOption<T>(id, value) {
            Text = text ?? DefaultTextSelector?.Invoke(value) ?? (value != null ? value.ToString() : null),
            IconName = iconName ?? DefaultIconNameSelector?.Invoke(value),
            IconAtlas = iconAtlas ?? DefaultIconAtlasSelector?.Invoke(value),
            IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? isEnabled,
            IsVisible = DefaultVisibleSelector?.Invoke(value) ?? isVisible,
        };

        _options.Add(option);
        _optionsById.Add(option.Id, option);
        OnOptionAdded(option);
        return option;
    }

    public void Bind(DropdownTriggerButton button) {
        if (TriggerButton != null || button == null) return;

        TriggerButton = button;
        TriggerButton.objectUserData = this;
        TriggerButton.PopupCreated += OnPopupCreated;
        TriggerButton.eventMouseWheel += OnTriggerButtonMouseWheel;

        if (_selectedOption != null)
            TriggerButton.SetTextWithoutNotify(TriggerButtonTextUpdated != null ? TriggerButtonTextUpdated(_selectedOption) : _selectedOption.Text);

        AddSelectableItems();
    }

    public void Unbind() {
        if (TriggerButton == null) return;

        TriggerButton.PopupCreated -= OnPopupCreated;
        TriggerButton.eventMouseWheel -= OnTriggerButtonMouseWheel;
        TriggerButton.objectUserData = null;
        TriggerButton = null;
    }

    protected void AddSelectableItem(SelectableOption<T> option) {
        if (ListBox == null) return;

        var item = ListBox.AddItem(option.Id);
        SetupSelectableItem(item, option);
        NotifySelectableItemCreated(item);
        ListBox.RefreshContentLayout();
    }

    protected void AddSelectableItems() {
        if (ListBox == null) return;

        ListBox.SuspendAutoLayout();
        foreach (var option in _options) {
            var item = ListBox.AddItem(option.Id);
            SetupSelectableItem(item, option);
            NotifySelectableItemCreated(item);
        }

        ListBox.ResumeAutoLayout();
        ListBox.RefreshContentLayout();
    }

    private void AddCanWheelOption(SelectableOption<T> option) {
        if (!option.IsVisible || !option.IsEnabled) return;

        _canWheelOptions.Add(option);
        _canWheelOptionsIndexById.Add(option.Id, _canWheelOptions.Count - 1);
    }

    private void OnPopupCreated(TextControl sender, SelectableListBox value) {
        PopupCreated?.Invoke(TriggerButton, value);
        AddSelectableItems();
    }

    private void OnSelectableItemClicked(UIComponent component, UIMouseEventParameter eventParam) {
        if (!component.isEnabled || !component.isVisible) return;

        if (component is SelectableItem item) {
            SetSelected(item.Id);
        }

        if (ClosePopupAfterSelected)
            TriggerButton?.HidePopup();
    }

    private void OnTriggerButtonMouseWheel(UIComponent component, UIMouseEventParameter eventParam) {
        if (!CanWheel || _canWheelOptions.Count <= 1 || _selectedOption == null) return;

        var dir = Math.Sign(eventParam.wheelDelta);
        if (dir == 0)
            return;

        if (!_canWheelOptionsIndexById.TryGetValue(_selectedOption.Id, out var visibleIndex)) {
            visibleIndex = 0;
        }

        var newVisibleIndex = Mathf.Clamp(visibleIndex - dir, 0, _canWheelOptions.Count - 1);

        var target = _canWheelOptions[newVisibleIndex];

        SetSelected(target);

        eventParam.Use();
    }

    private void SetupSelectableItem(SelectableItem item, SelectableOption<T> option) {
        item.SuspendAutoLayout();
        item.objectUserData = option;
        item.eventClicked += OnSelectableItemClicked;
        item.Text = option.Text;
        item.IconSpriteName = option.IconName;
        item.IconAtlas = option.IconAtlas;
        item.isEnabled = option.IsEnabled;
        item.isVisible = option.IsVisible;
        item.SetSelectionWithoutNotify(option.IsSelected);
        item.ResumeAutoLayout();
    }

    private void UpdateCanWheelOption() {
        _canWheelOptions.Clear();
        _canWheelOptionsIndexById.Clear();
        foreach (var option in _options) {
            AddCanWheelOption(option);
        }
    }
}