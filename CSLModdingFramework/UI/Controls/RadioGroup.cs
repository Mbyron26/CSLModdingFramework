namespace CSLModdingFramework.UI.Controls;

public class RadioGroup<T> : RadioGroupBase<T> {
    private RadioButtonList _boundRadioButtonList;

    public override RadioButtonList BoundRadioButtonList => _boundRadioButtonList;

    public void Bind(RadioButtonList radioButtonList) {
        if (BoundRadioButtonList != null || radioButtonList == null) return;

        _boundRadioButtonList = radioButtonList;
        _boundRadioButtonList.objectUserData = this;
        _boundRadioButtonList.ItemCreated += OnRadioButtonCreated;
        _boundRadioButtonList.ItemDestroying += OnRadioDestroying;

        RebuildRadioButtonList();
    }

    public void Unbind() {
        if (_boundRadioButtonList == null) return;

        foreach (var item in _boundRadioButtonList.Items) {
            item.objectUserData = null;
            item.eventClicked -= OnRadioButtonClicked;
        }

        _boundRadioButtonList.ItemCreated -= OnRadioButtonCreated;
        _boundRadioButtonList.ItemDestroying -= OnRadioDestroying;
        _boundRadioButtonList.objectUserData = null;
        _boundRadioButtonList = null;
    }
}