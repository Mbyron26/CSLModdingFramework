using System;
using CSLModdingFramework.Core;
using CSLModdingFramework.Localization;
using CSLModdingFramework.UI.Controls;

namespace CSLModdingFramework.UI.Dialogs;

public class MessageDialog : Dialog {
    public TextLabel ContentTextLabel { get; private set; }

    protected override void OnAwake() {
        base.OnAwake();

        _scrollViewer.MaxFitSize = new Size(width, 400);
        _contentContainer.Padding = new EdgeInsets(0, 0, 10, 10);
        ContentTextLabel = _contentContainer.AddUIComponent<TextLabel>();
        ContentTextLabel.SuspendAutoLayout();
        ContentTextLabel.width = _scrollViewer.width;
        ContentTextLabel.TextWordWrap = true;
        ContentTextLabel.SizeMode = TextSizeMode.FitHeight;
        ContentTextLabel.ResumeAutoLayout();
    }

    public static MessageDialog ShowMessageDialog(ModDomain modDomain, string message, Action<MessageDialog> setup = null) => Show<MessageDialog>(dialog => {
        dialog.SuspendAutoLayout();
        dialog.Title = modDomain.ModSystem.ModName;
        dialog.ContentTextLabel.Text = message;
        var okButton = dialog.AddButton(modDomain.ModSystem.ModLocalizationSystem.GetString(LocalizationStrings.OK));
        okButton.Clicked += () => Close(dialog);
        setup?.Invoke(dialog);
        dialog.ResumeAutoLayout();
        dialog.CenterToParent();
    });

    public static MessageDialog ShowMessageDialog(string title, string message, string confirmText = "OK", Action<MessageDialog> setup = null) => Show<MessageDialog>(dialog => {
        dialog.SuspendAutoLayout();
        dialog.Title = title;
        dialog.ContentTextLabel.Text = message;
        var button = dialog.AddButton(confirmText );
        button.Clicked += () => Close(dialog);
        setup?.Invoke(dialog);
        dialog.ResumeAutoLayout();
        dialog.CenterToParent();
    });
}