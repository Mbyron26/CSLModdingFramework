using CSLModdingFramework.Localization;

namespace CSLModdingFramework.UI.Controls;

public class DropdownCheckboxGroup<T> : CheckboxGroupBase<T> {
    public override CheckboxList BoundCheckboxList => TriggerButton?.Popup.List;

    public DropdownCheckboxTriggerButton TriggerButton { get; private set; }

    protected override void UpdateSelectAllCheckBoxState() {
        if (BoundCheckboxList == null) return;

        var selectAllCheckBox = BoundCheckboxList.SelectAllItem;
        if (selectAllCheckBox == null) return;

        var (allCount, checkedCount) = GetSelectionCounts();
        UpdateTriggerButtonText(checkedCount);

        if (checkedCount == 0) {
            selectAllCheckBox.SetCheckStateWithoutNotify(CheckState.Unchecked);
        }
        else if (allCount == checkedCount) {
            selectAllCheckBox.SetCheckStateWithoutNotify(CheckState.Checked);
        }
        else {
            selectAllCheckBox.SetCheckStateWithoutNotify(CheckState.Indeterminate);
        }
    }

    public void Bind(DropdownCheckboxTriggerButton dropdownCheckBoxTriggerButton) {
        if (TriggerButton != null || dropdownCheckBoxTriggerButton == null) return;

        TriggerButton = dropdownCheckBoxTriggerButton;
        UpdateTriggerButtonText();
        TriggerButton.PopupCreated += OnTriggerButtonPopupCreatedSetup;
    }

    public void Unbind() {
        if (TriggerButton == null) return;

        var checkBoxList = TriggerButton.Popup.List;
        if (checkBoxList == null) return;

        foreach (var item in checkBoxList.Items) {
            item.eventClicked -= OnCheckboxClicked;
        }

        var selectAllCheckBox = checkBoxList.SelectAllItem;
        if (selectAllCheckBox != null)
            selectAllCheckBox.eventClicked -= OnSelectAllCheckboxClicked;

        checkBoxList.objectUserData = null;
        TriggerButton = null;
    }

    private void OnTriggerButtonPopupCreatedSetup(TextControl sender, ScrollableCheckboxListBox value) {
        value.List.objectUserData = this;

        if (AutoAddSelectAllCheckBox) {
            AddSelectAllCheckBox();
        }

        RebuildCheckboxList();
    }

    private void UpdateTriggerButtonText() {
        if (TriggerButton == null) return;

        var (_, checkedCount) = GetSelectionCounts();
        UpdateTriggerButtonText(checkedCount);
    }

    private void UpdateTriggerButtonText(int checkedCount) {
        if (checkedCount == 0) {
            TriggerButton.Text = LocalizationSystem is not null ? LocalizationSystem.GetString(LocalizationStrings.SelectOptions) : LocalizationStrings.SelectOptions.DefaultValue;
        }
        else {
            TriggerButton.Text =  $"{checkedCount} {(LocalizationSystem is not null ? LocalizationSystem.GetString(LocalizationStrings.Selected) : LocalizationStrings.Selected.DefaultValue)}";
        }
    }
}