using CSLModdingFramework.UI.Controls;

namespace CSLModdingFramework.UI;

public class DropdownCheckboxTriggerButton : PopupTrigger<ScrollableCheckboxListBox> {
    protected override ScrollableCheckboxListBox GetOrCreatePopup() {
        if (Popup != null) return Popup;

        var rootContainer = GetRootContainer();
        Popup = rootContainer.AddUIComponent<ScrollableCheckboxListBox>();
        Popup.size = size;
        Popup.List.width = width - 2 * 8;

        NotifyPopupCreated(this, Popup);
        return Popup;
    }
}