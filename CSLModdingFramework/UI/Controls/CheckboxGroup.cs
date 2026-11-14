namespace CSLModdingFramework.UI.Controls;

public class CheckboxGroup<T> : CheckboxGroupBase<T> {
    private CheckboxList _listBox;

    public override CheckboxList BoundCheckboxList => _listBox;

    public void Bind(CheckboxList checkboxList) {
        if (_listBox != null || checkboxList == null) return;

        _listBox = checkboxList;
        _listBox.objectUserData = this;

        if (AutoAddSelectAllCheckBox && _listBox.SelectAllItem == null) {
            AddSelectAllCheckBox();
        }

        RebuildCheckboxList();
    }

    public void Unbind() {
        if (_listBox == null) return;

        foreach (var item in _listBox.Items) {
            item.eventClicked -= OnCheckboxClicked;
        }

        if (_listBox.SelectAllItem != null)
            _listBox.SelectAllItem.eventClicked -= OnSelectAllCheckboxClicked;

        _listBox.objectUserData = null;
        _listBox = null;
    }
}