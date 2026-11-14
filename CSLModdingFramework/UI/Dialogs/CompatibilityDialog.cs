using System;
using System.Linq;
using ColossalFramework.UI;
using CSLModdingFramework.Core;
using CSLModdingFramework.Core.Compatibility;
using CSLModdingFramework.Localization;
using CSLModdingFramework.UI.Controls;
using UnityEngine;

namespace CSLModdingFramework.UI.Dialogs;

public class CompatibilityDialog : Dialog {
    private ModSystemBase _modSystemBase;
    private CompatibilitySystem _compatibilitySystem;
    private LocalizationSystem _localizationSystem;

    private void FillContent(ModDomain modDomain) {
        _modSystemBase = modDomain.ModSystem;
        _compatibilitySystem = modDomain.ModSystem.CompatibilitySystem;
        _localizationSystem = modDomain.ModSystem.ModLocalizationSystem;
        Title = $"{_modSystemBase.ModName} {_localizationSystem.GetString(LocalizationStrings.CompatibilityCheck)}";

        _contentContainer.RowGap = 20;
        AddDependenciesContent();
        AddGameVersionContent();
        AddCompatibilityContent();

        if (_compatibilitySystem.IncompatibleRule.IsMatched) {
            var button = AddButton(_localizationSystem.GetString(LocalizationStrings.ResolveConflicts));
            button.Clicked += OnHandleIncompatibleMods;
        }

        if (_compatibilitySystem.ShouldRestartGame) {
            var restartGameLabel = _contentContainer.AddUIComponent<TextLabel>();
            restartGameLabel.SuspendAutoLayout();
            restartGameLabel.width = _contentContainer.width;
            restartGameLabel.SizeMode = TextSizeMode.FitHeight;
            restartGameLabel.TextWordWrap = true;
            restartGameLabel.Text = _localizationSystem.GetString(LocalizationStrings.RestartGame);
            restartGameLabel.ResumeAutoLayout();
        }

        var button2 = AddButton(_localizationSystem.GetString(LocalizationStrings.OK));
        button2.Clicked += Close;
        InvalidateAutoLayout();
        CenterToParent();
    }

    private void OnHandleIncompatibleMods() {
        Close(this);
        _compatibilitySystem.OnHandleIncompatibleMods();
    }

    private void AddCompatibilityContent() {
        var isMatched = _compatibilitySystem.IncompatibleRule.IsMatched;
        var shouldRestartGame = _compatibilitySystem.ShouldRestartGame;
        var description = isMatched ? _localizationSystem.GetString(LocalizationStrings.CompatibilityWarning) : shouldRestartGame ? _localizationSystem.GetString(LocalizationStrings.GameRestartRequired) : _localizationSystem.GetString(LocalizationStrings.NoIncompatibleModsDetected);
        AddItem(isMatched, _localizationSystem.GetString(LocalizationStrings.IncompatibleMods), description, v => v.SpriteName = isMatched ? DefaultAtlas.Warn : shouldRestartGame ? DefaultAtlas.Warn : DefaultAtlas.Correct);

        if (!isMatched) return;

        foreach (var incompatibleModItem in _compatibilitySystem.IncompatibleRule.Lookup.Values)
            if (incompatibleModItem.IsMatched) {
                AddIncompatibleModItem(incompatibleModItem);
            }
    }

    private SettingsSection _settingsSection;

    private void AddIncompatibleModItem(IncompatibleModItem incompatibleModItem) {
        if (incompatibleModItem is null) return;

        if (_settingsSection == null) {
            _settingsSection = _contentContainer.AddUIComponent<SettingsSection>();
            _settingsSection.SuspendAutoLayout();
            _settingsSection.width = _contentContainer.width;
            _settingsSection.Theme = ThemeKind.ClassicDark;
            _settingsSection.DescriptionScale = 0.8f;
            _settingsSection.ItemLayoutPadding = new EdgeInsets(16, 16, 8, 8);
            _settingsSection.ResumeAutoLayout();
        }

        var text = incompatibleModItem.IncompatibilityLevel == IncompatibilityModLevel.EnableNotAllowed ? _localizationSystem.GetString(LocalizationStrings.EnableNotAllowed) : _localizationSystem.GetString(LocalizationStrings.LoadNotAllowed);
        var header = incompatibleModItem.DisplayName ?? string.Empty;
        string description;
        if (string.IsNullOrEmpty(incompatibleModItem.CustomWarningMessage))
            description = (incompatibleModItem.IsDuplicateFunctionality ? _localizationSystem.GetString(LocalizationStrings.DuplicateFunctionality) : _localizationSystem.GetString(LocalizationStrings.IncompatibleWithMod)) + " " + (string.IsNullOrEmpty(incompatibleModItem.AlternativeModName) ? string.Empty : _localizationSystem.GetString(LocalizationStrings.AlternativeMod) + $": {incompatibleModItem.AlternativeModName}");
        else
            description = _localizationSystem.GetString(incompatibleModItem.CustomWarningMessage);

        var textLabel = _settingsSection.AddTextLabel(header, text, description);

        textLabel.Background.Atlas = DefaultAtlas.Instance;
        textLabel.Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
        textLabel.Background.Colors.SetValues(incompatibleModItem.IncompatibilityLevel == IncompatibilityModLevel.EnableNotAllowed ? new Color32(216, 136, 30, 255) : new Color32(146, 4, 10, 255));
        textLabel.TextPadding = new EdgeInsets(10, 10, 5, 5);
    }

    private void AddGameVersionContent() {
        var isMatched = _compatibilitySystem.VersionRule.IsMatched;
        var infoContainer = AddItem(isMatched, _localizationSystem.GetString(LocalizationStrings.MakeForCurrentGameVersion), isMatched ? $"{_localizationSystem.GetString(LocalizationStrings.NotMakeForCurrentGameVersion)}! {_localizationSystem.GetString(LocalizationStrings.CompatibleGameVersions)}:" : _localizationSystem.GetString(LocalizationStrings.CompatibleCurrentGameVersion));

        if (!isMatched) return;

        var versionCompatibility = _compatibilitySystem.VersionRule.VersionCompatibility;
        var dependenciesItems = infoContainer.AddUIComponent<TextLabel>();
        dependenciesItems.width = infoContainer.width;
        dependenciesItems.TextWordWrap = true;
        dependenciesItems.TextHorizontalAlignment = UIHorizontalAlignment.Left;
        dependenciesItems.Text = versionCompatibility.ToString();
    }

    private void AddDependenciesContent() {
        var isMatched = _compatibilitySystem.DependencyRule.IsMatched;
        var infoContainer = AddItem(isMatched, _localizationSystem.GetString(LocalizationStrings.Dependencies), isMatched ? $"{_localizationSystem.GetString(LocalizationStrings.MissingDependencies)}! {_localizationSystem.GetString(LocalizationStrings.SubscribeDependencies)}:" : _localizationSystem.GetString(LocalizationStrings.NoMissingDependencies));

        if (!isMatched) return;

        var matchedDependencyNames = _compatibilitySystem.DependencyRule.Lookup.Select(v => v.DisplayName).ToArray();
        var dependenciesItems = infoContainer.AddUIComponent<TextLabel>();
        dependenciesItems.width = infoContainer.width;
        dependenciesItems.TextWordWrap = true;
        dependenciesItems.Text = string.Join(", ", matchedDependencyNames);
    }

    private LiteContainer AddItem(bool isMatched, string header, string description, Action<SpriteView> flagIconAction = null) {
        var container = _contentContainer.AddUIComponent<LiteContainer>();
        container.SuspendAutoLayout();
        container.width = _contentContainer.width;
        container.Direction = FlexDirection.Row;
        container.AutoLayout = true;
        container.AutoFitChildrenVertically = true;
        container.ColumnGap = 10;
        container.Padding = new EdgeInsets(10);
        var containerBackground = container.Background;
        containerBackground.Atlas = DefaultAtlas.Instance;
        containerBackground.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
        containerBackground.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingsSectionBackground);
        containerBackground.Colors.Hovered = UIColorPalette.ClassicDarkPalette.SettingsSectionBackgroundHovered;

        var flagIcon = container.AddUIComponent<SpriteView>();
        flagIcon.size = new Vector2(32, 32);
        flagIcon.Atlas = DefaultAtlas.Instance;
        flagIcon.SpriteName = isMatched ? DefaultAtlas.Warn : DefaultAtlas.Correct;
        flagIconAction?.Invoke(flagIcon);

        var infoContainer = container.AddUIComponent<LiteContainer>();
        infoContainer.SuspendAutoLayout();
        infoContainer.width = container.width - flagIcon.width - container.ColumnGap - container.Padding.Horizontal;
        infoContainer.AutoLayout = true;
        infoContainer.Direction = FlexDirection.Column;
        infoContainer.AutoFitChildrenVertically = true;
        infoContainer.RowGap = 10;

        var headerElement = infoContainer.AddUIComponent<TextLabel>();
        headerElement.SuspendAutoLayout();
        headerElement.width = infoContainer.width;
        headerElement.TextHorizontalAlignment = UIHorizontalAlignment.Left;
        headerElement.Text = header;
        headerElement.ResumeAutoLayout();

        var descriptionElement = infoContainer.AddUIComponent<TextLabel>();
        descriptionElement.SuspendAutoLayout();
        descriptionElement.width = infoContainer.width;
        descriptionElement.TextWordWrap = true;
        descriptionElement.TextHorizontalAlignment = UIHorizontalAlignment.Left;
        descriptionElement.SizeMode = TextSizeMode.FitHeight;
        descriptionElement.Text = description;
        descriptionElement.TextColors.SetValues(UIColorPalette.DescriptionTextColors);
        descriptionElement.ResumeAutoLayout();

        infoContainer.ResumeAutoLayout();
        container.ResumeAutoLayout();

        return infoContainer;
    }

    public static CompatibilityDialog ShowCompatibilityDialog(ModDomain domain) => Show<CompatibilityDialog>(v => v.FillContent(domain));
}