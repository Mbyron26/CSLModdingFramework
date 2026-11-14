using CSLModdingFramework.UI.Controls;

namespace CSLModdingFramework.UI;

public class DropdownTriggerButton : PopupTrigger<SelectableListBox> {
    protected override SelectableListBox GetOrCreatePopup() {
        if (Popup != null) return Popup;

        var rootContainer = GetRootContainer();
        Popup = rootContainer.AddUIComponent<SelectableListBox>();
        Popup.size = size;
        NotifyPopupCreated(this, Popup);
        return Popup;
    }
}