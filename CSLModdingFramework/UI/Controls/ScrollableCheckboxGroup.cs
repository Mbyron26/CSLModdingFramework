namespace CSLModdingFramework.UI.Controls;

public class ScrollableCheckboxGroup<T> : CheckboxGroupBase<T> {
    public ScrollableCheckboxListBox View { get; private set; }
    public override CheckboxList BoundCheckboxList => View?.Content as CheckboxList;

    public void Bind(ScrollableCheckboxListBox scrollableCheckBoxList) {
        if (View != null || scrollableCheckBoxList == null) return;

        View = scrollableCheckBoxList;
        View.List.objectUserData = this;

        if (AutoAddSelectAllCheckBox) {
            AddSelectAllCheckBox();
        }

        RebuildCheckboxList();
    }

    public void Unbind() {
        if (View == null) return;

        var checkBoxList = View.Content as CheckboxList;
        if (checkBoxList == null) return;

        foreach (var item in checkBoxList.Items) {
            item.eventClicked -= OnCheckboxClicked;
        }

        var selectAllCheckBox = checkBoxList.SelectAllItem;
        if (selectAllCheckBox != null)
            selectAllCheckBox.eventClicked -= OnSelectAllCheckboxClicked;

        checkBoxList.objectUserData = null;
        View = null;
    }
}