using System;
using CSLModdingFramework.Core;
using CSLModdingFramework.Localization;
using CSLModdingFramework.UI.Controls;

namespace CSLModdingFramework.UI.Dialogs;

public class ConfirmDialog : Dialog {
    public TextLabel ContentTextLabel { get; private set; }

    protected override void OnAwake() {
        base.OnAwake();

        _scrollViewer.MaxFitSize = new Size(width, 400);
        _contentContainer.Padding = new EdgeInsets(0, 0, 10, 10);
        ContentTextLabel = _contentContainer.AddUIComponent<TextLabel>();
        ContentTextLabel.SuspendAutoLayout();
        ContentTextLabel.width = _contentContainer.width;
        ContentTextLabel.TextWordWrap = true;
        ContentTextLabel.SizeMode = TextSizeMode.FitHeight;
        ContentTextLabel.ResumeAutoLayout();
    }

    public static ConfirmDialog ShowConfirmDialog(ModDomain modDomain, string message, Action<ConfirmDialog> confirm, Action<ConfirmDialog> cancel = null, Action<ConfirmDialog> setup = null) => Show<ConfirmDialog>(dialog => {
        dialog.SuspendAutoLayout();
        dialog.Title = modDomain.ModSystem.ModName;
        dialog.ContentTextLabel.Text = message;
        var localizationSystem = modDomain.ModSystem.ModLocalizationSystem;
        var confirmButton = dialog.AddButton(localizationSystem.GetString(LocalizationStrings.OK));
        confirmButton.Clicked += () => confirm?.Invoke(dialog);
        var cancelButton = dialog.AddButton(localizationSystem.GetString(LocalizationStrings.Cancel));
        cancelButton.Clicked += () => cancel?.Invoke(dialog);
        setup?.Invoke(dialog);
        dialog.ResumeAutoLayout();
        dialog.CenterToParent();
    });

    public static ConfirmDialog ShowConfirmDialog(string title, string message, string confirmText, string cancelText, Action<ConfirmDialog> confirm, Action<ConfirmDialog> cancel = null, Action<ConfirmDialog> setup = null) => Show<ConfirmDialog>(dialog => {
        dialog.SuspendAutoLayout();
        dialog.Title = title;
        dialog.ContentTextLabel.Text = message;
        var confirmButton = dialog.AddButton(confirmText);
        confirmButton.Clicked += () => confirm?.Invoke(dialog);
        var cancelButton = dialog.AddButton(cancelText);
        cancelButton.Clicked += () => cancel?.Invoke(dialog);
        setup?.Invoke(dialog);
        dialog.ResumeAutoLayout();
        dialog.CenterToParent();
    });
}