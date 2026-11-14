using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using CSLModdingFramework.Localization;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class CheckboxGroupBase<T> : CheckboxOptionsGroupBase<T> {
    public event Action<Checkbox> CheckboxCreated;
    public event Action<Checkbox> SelectAllCheckboxCreated;

    public abstract CheckboxList BoundCheckboxList { get; }

    public bool AutoAddSelectAllCheckBox { get; set; } = true;
    public bool IsIconOnlyClickable { get; set; } = true;
    public ILocalizationProvider LocalizationSystem { get; set; }

    public virtual void AddSelectAllCheckBox(Action<Checkbox> setup = null) {
        if (BoundCheckboxList == null) return;

        var selectAllCheckBox = BoundCheckboxList.SelectAllItem;
        if (selectAllCheckBox != null) return;

        var checkbox = AddSelectAllCheckBox(BoundCheckboxList);
        UpdateSelectAllCheckBoxState();
        setup?.Invoke(checkbox);
    }

    protected virtual void AddCheckbox(CheckboxOption<T> option) {
        var checkbox = BoundCheckboxList.AddItem(option.Id);
        checkbox.objectUserData = option;
        checkbox.eventClicked += OnCheckboxClicked;
        checkbox.Header = option.Header;
        checkbox.Description = option.Description;
        checkbox.CheckState = option.IsChecked ? CheckState.Checked : CheckState.Unchecked;
        checkbox.isEnabled = option.IsEnabled;
        checkbox.isVisible = option.IsVisible;
        NotifyCheckBoxSetup(checkbox);
    }

    protected virtual void OnCheckboxClicked(UIComponent component, UIMouseEventParameter eventParam) {
        var checkbox = component as Checkbox;
        if (checkbox == null) return;

        if (IsIconOnlyClickable) {
            var iconRect = new Rect(new Vector2(checkbox.LayoutPadding.Left, checkbox.LayoutPadding.Top), checkbox.IndicatorSize);
            if (!checkbox.GetHitPosition(eventParam.ray, out var hitPosition)) return;

            if (iconRect.Contains(hitPosition)) {
                ToggleCheckBox(checkbox);
            }
        }
        else {
            ToggleCheckBox(checkbox);
        }
    }

    protected virtual void OnSelectAllCheckboxClicked(UIComponent component, UIMouseEventParameter eventParam) {
        var checkbox = component as Checkbox;
        if (checkbox == null) return;

        if (IsIconOnlyClickable) {
            var iconRect = new Rect(new Vector2(checkbox.LayoutPadding.Left, checkbox.LayoutPadding.Top), checkbox.IndicatorSize);
            if (!checkbox.GetHitPosition(eventParam.ray, out var hitPosition)) return;

            if (iconRect.Contains(hitPosition)) {
                ToggleAll();
            }
        }
        else {
            ToggleAll();
        }
    }

    protected virtual void RebuildCheckboxList() {
        if (BoundCheckboxList == null) return;

        BoundCheckboxList.DestroyItems();
        foreach (var option in _options) {
            AddCheckbox(option);
        }

        UpdateSelectAllCheckBoxState();
    }

    protected virtual void ToggleCheckBox(Checkbox checkbox) {
        if (checkbox.CheckState == CheckState.Checked) {
            checkbox.SetCheckStateWithoutNotify(CheckState.Unchecked);
            SetChecked(checkbox.Id, false);
        }
        else if (checkbox.CheckState == CheckState.Unchecked) {
            checkbox.SetCheckStateWithoutNotify(CheckState.Checked);
            SetChecked(checkbox.Id, true);
        }
    }

    protected virtual void UpdateSelectAllCheckBoxState() {
        if (BoundCheckboxList == null) return;

        var selectAllCheckBox = BoundCheckboxList.SelectAllItem;
        if (selectAllCheckBox == null) return;

        var (allCount, checkCount) = GetSelectionCounts();
        if (checkCount == 0) {
            selectAllCheckBox.SetCheckStateWithoutNotify(CheckState.Unchecked);
        }
        else if (allCount == checkCount) {
            selectAllCheckBox.SetCheckStateWithoutNotify(CheckState.Checked);
        }
        else {
            selectAllCheckBox.SetCheckStateWithoutNotify(CheckState.Indeterminate);
        }
    }

    protected override void OnOptionAdded(CheckboxOption<T> option) {
        if (BoundCheckboxList == null) return;

        AddCheckbox(option);
        UpdateSelectAllCheckBoxState();
    }

    protected override void OnOptionCheckedChanged(CheckboxOption<T> option, bool notify = true) {
        if (BoundCheckboxList != null && BoundCheckboxList.ItemsById.TryGetValue(option.Id, out var checkbox)) {
            checkbox.CheckState = option.IsChecked ? CheckState.Checked : CheckState.Unchecked;
            UpdateSelectAllCheckBoxState();
        }

        base.OnOptionCheckedChanged(option, notify);
    }

    protected override void OnOptionEnabledChanged(CheckboxOption<T> option) {
        if (BoundCheckboxList != null && BoundCheckboxList.ItemsById.TryGetValue(option.Id, out var checkbox)) {
            checkbox.isEnabled = option.IsEnabled;
        }

        UpdateSelectAllCheckBoxState();
    }

    protected override void OnOptionRemoved(CheckboxOption<T> option) {
        if (BoundCheckboxList != null && BoundCheckboxList.ItemsById.TryGetValue(option.Id, out var checkbox)) {
            checkbox.eventClicked -= OnCheckboxClicked;
            BoundCheckboxList.DestroyItem(checkbox.Id);
        }

        UpdateSelectAllCheckBoxState();
    }

    protected override void OnOptionVisibleChanged(CheckboxOption<T> option) {
        if (BoundCheckboxList != null && BoundCheckboxList.ItemsById.TryGetValue(option.Id, out var checkbox)) {
            checkbox.isVisible = option.IsVisible;
        }

        UpdateSelectAllCheckBoxState();
    }

    protected override void OnOptionsAdded(List<CheckboxOption<T>> options) {
        if (BoundCheckboxList == null) return;

        BoundCheckboxList.SuspendAutoLayout();

        foreach (var option in options) {
            AddCheckbox(option);
        }

        BoundCheckboxList.ResumeAutoLayout();
    }

    protected override void OnOptionsCheckedChanged(List<CheckboxOption<T>> options, bool notify = true) {
        if (BoundCheckboxList != null) {
            foreach (var option in options) {
                if (BoundCheckboxList.ItemsById.TryGetValue(option.Id, out var checkbox)) {
                    checkbox.CheckState = option.IsChecked ? CheckState.Checked : CheckState.Unchecked;
                }
            }

            UpdateSelectAllCheckBoxState();
        }

        base.OnOptionsCheckedChanged(options, notify);
    }

    protected override void OnOptionsEnabledChanged(List<CheckboxOption<T>> options) {
        if (BoundCheckboxList != null) {
            foreach (var option in options) {
                if (BoundCheckboxList.ItemsById.TryGetValue(option.Id, out var checkbox)) {
                    checkbox.isEnabled = option.IsEnabled;
                }
            }

            UpdateSelectAllCheckBoxState();
        }
    }

    protected override void OnOptionsRemoved(List<CheckboxOption<T>> removedOptions) {
        if (BoundCheckboxList == null) return;

        BoundCheckboxList.SuspendAutoLayout();

        foreach (var option in removedOptions) {
            BoundCheckboxList.DestroyItem(option.Id, checkbox => checkbox.eventClicked -= OnCheckboxClicked);
        }

        BoundCheckboxList.ResumeAutoLayout();

        UpdateSelectAllCheckBoxState();
    }

    protected override void OnOptionsSorted() => OnOptionsRemoved(_options);

    protected override void OnOptionsVisibleChanged(List<CheckboxOption<T>> changedOptions) {
        if (BoundCheckboxList != null) {
            foreach (var option in changedOptions) {
                if (BoundCheckboxList.ItemsById.TryGetValue(option.Id, out var checkbox)) {
                    checkbox.isVisible = option.IsVisible;
                }
            }

            UpdateSelectAllCheckBoxState();
        }
    }

    protected Checkbox AddSelectAllCheckBox(CheckboxList checkBoxList) {
        var checkbox = checkBoxList.AddSelectAllItem();
        checkbox.eventClicked += OnSelectAllCheckboxClicked;
        checkbox.Header = LocalizationSystem is not null ? LocalizationSystem.GetString(LocalizationStrings.SelectAllOrDeselectAll) : "Select All / Deselect All";
        NotifySelectAllCheckBoxCreated(checkbox);
        return checkbox;
    }

    protected void NotifyCheckBoxSetup(Checkbox checkbox) => CheckboxCreated?.Invoke(checkbox);

    protected void NotifySelectAllCheckBoxCreated(Checkbox checkbox) => SelectAllCheckboxCreated?.Invoke(checkbox);
}