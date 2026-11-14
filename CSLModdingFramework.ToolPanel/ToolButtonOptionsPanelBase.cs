using CSLModdingFramework.Localization;
using CSLModdingFramework.UI;
using CSLModdingFramework.UI.Controls;
using CSLModdingFramework.UI.Dialogs;
using UnityEngine;

namespace CSLModdingFramework.ToolPanel;

public abstract class ToolButtonOptionsPanelBase : OptionsPanelBase {
    protected abstract UnifiedUIToolPanelSystemBase UnifiedUIToolPanelSystem { get; }

    protected virtual void AddInGameToolButtonSection(LiteContainer page) {
        if (_settingsSystem.MainSettings is not ToolButtonModSettingsBase modSettings) return;

        var toolButtonSection = AddSection(page, _localizationManager.GetString(LocalizationStrings.ToolButton));

        var uuiEnabled = UnifiedUIToolPanelSystem.IsUnifiedUIModEnabled;
        if (!uuiEnabled) {
            modSettings.ToolButtonDisplayScope = (ToolButtonScope)Mathf.Clamp((int)modSettings.ToolButtonDisplayScope, 0, 1);
        }

        var toolButtonDropdown = toolButtonSection.AddDropdown<ToolButtonScope>(_localizationManager.GetString(LocalizationStrings.ToolButtonDisplay));
        toolButtonDropdown.CanWheel = false;
        toolButtonDropdown.AddOption(ToolButtonScope.None, _localizationManager.GetString(LocalizationStrings.None));
        toolButtonDropdown.AddOption(ToolButtonScope.InGame, _localizationManager.GetString(LocalizationStrings.OnlyInGame));
        if (uuiEnabled) {
            toolButtonDropdown.AddOption(ToolButtonScope.UnifiedUI, _localizationManager.GetString(LocalizationStrings.OnlyInUUI));
            toolButtonDropdown.AddOption(ToolButtonScope.Both, _localizationManager.GetString(LocalizationStrings.Both));
        }

        toolButtonDropdown.SetSelectedWithoutNotify(v => v.Value == modSettings.ToolButtonDisplayScope);
        toolButtonDropdown.ValueSelectionChanged += OnToolButtonDropdownValueSelectionChanged;

        toolButtonSection.AddButton(_localizationManager.GetString(LocalizationStrings.ResetToolButtonPosition), null, _localizationManager.GetString(LocalizationStrings.Reset), ResetToolButtonPosition);
    }

    private void OnToolButtonDropdownValueSelectionChanged(ToolButtonScope obj) {
        if (_settingsSystem.MainSettings is not ToolButtonModSettingsBase modSettings) return;

        modSettings.ToolButtonDisplayScope = obj;
        UnifiedUIToolPanelSystem?.OnToolButtonSelectionChanged();
    }

    private void ResetToolButtonPosition() => ConfirmDialog.ShowConfirmDialog(_domain, _localizationManager.GetString(LocalizationStrings.ResetToolButtonPosition), d => {
        d.Close();
        UnifiedUIToolPanelSystem?.SetInGameToolButtonDefaultPosition();
        MessageDialog.ShowMessageDialog(_domain, _localizationManager.GetString(LocalizationStrings.ToolButtonPositionResetSuccessMessage));
    }, cancel => cancel.Close());
}