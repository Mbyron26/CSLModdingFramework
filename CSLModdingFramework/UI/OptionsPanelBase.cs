using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Configuration;
using CSLModdingFramework.Core;
using CSLModdingFramework.Core.Compatibility;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Localization;
using CSLModdingFramework.Logging;
using CSLModdingFramework.UI.Controls;
using CSLModdingFramework.UI.Dialogs;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.Utilities;
using UnityEngine;
using Logger = CSLModdingFramework.Logging.Logger;

namespace CSLModdingFramework.UI;

public abstract class OptionsPanelBase : Container, IThemeable {
    public const string Advanced = nameof(Advanced);
    public const float BasePadding = 16;
    public const string General = nameof(General);
    public const float TabHeight = 30;
    public static readonly Vector2 FixedSize = new(764, 773);
    protected LiteContainer _advancedPageContainer;
    protected LiteContainer _debugPageContainer;
    protected ModDomain _domain;
    protected LiteContainer _generalPageContainer;
    protected LiteContainer _keyBindingPageContainer;
    protected LocalizationSystem _localizationManager;
    protected ILog _logger;
    protected ModSystemBase _modSystem;
    protected SettingsSystem _settingsSystem;
    protected TabBar _tabBar;
    protected ThemeKind _theme;
    private SettingItem _floatArrayPoolSettingItem;
    private SettingItem _fontPoolSettingItem;
    private SettingItem _listPoolSettingItem;
    public static Vector2 PageSize => new(SectionWidth, FixedSize.y - 2 * BasePadding - TabHeight - 10);
    public static float SectionWidth => FixedSize.x - 2 * BasePadding;

    public abstract OptionsPanelSystemBase OptionsPanelSystem { get; }
    public override bool canFocus => isEnabled && isVisible;

    public ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    protected abstract ModDomain GetDomain();

    public virtual void SetTheme(ThemeKind theme) {
        if (theme.Equals(_theme)) return;

        _theme = theme;
        Background.Atlas = DefaultAtlas.Instance;

        switch (theme) {
            case ThemeKind.ClassicLight:
            case ThemeKind.ClassicDark:
                Background.SpriteName = DefaultAtlas.CustomBackground;
                Background.NormalColor = new Color32(130, 130, 130, 255);
                break;
            case ThemeKind.ModernDark:
            default:
                Background.SpriteName = DefaultAtlas.SmoothRectangleRadius12;
                Background.NormalColor = UIColorPalette.ModernDarkPalette.RootBackground;
                break;
        }
    }

    protected virtual void AddTabs() {
        _keyBindingPageContainer = AddTab(LocalizationStrings.KeyBinding, Localize(LocalizationStrings.KeyBinding));
        FillKeyBindingPage(_keyBindingPageContainer);
    }

    protected virtual void FillAdvancedPage(LiteContainer advancedPage) { }

    protected virtual void FillDebugPage(LiteContainer debugPage) {
        var section = AddSection(_debugPageContainer);
        section.AddTextLabel("CSLModdingFramework Version", FrameworkManager.CurrentVersion.ToDisplayString());
        section.AddToggle("Show Errors In UI", null, _logger.ShowsErrorsInUI, v => _logger.ShowsErrorsInUI = v);
        section.AddButton("Open log", null, "Open log", OpenLogFile);
        section.AddButton("Open game log", null, "Open game log", OpenGameLogFile);
        section.AddButton("Open settings directory", null, "Open settings directory", OpenSettingsDirectory);
        section.AddButton("Open mod setting file", null, "Open mod setting file", OpenModSettingFile);
        section.AddButton("Open mod logs directory", null, "Open mod logs directory", OpenModLogsDirectory);

        var section1 = AddSection(_debugPageContainer);
        _fontPoolSettingItem = section1.AddButton(GetFontPoolString(), null, "Refresh", () => _fontPoolSettingItem.Header = GetFontPoolString()).parent as SettingItem;
        _floatArrayPoolSettingItem = section1.AddButton(ArrayPool<float>.Shared.DumpStatistics(), null, "Refresh", () => _floatArrayPoolSettingItem.Header = ArrayPool<float>.Shared.DumpStatistics()).parent as SettingItem;
        _listPoolSettingItem = section1.AddButton(GetListPoolCount(), null, "Refresh", () => _listPoolSettingItem.Header = GetListPoolCount()).parent as SettingItem;
    }

    protected virtual void FillGeneralPage(LiteContainer generalPage) { }
    protected virtual void FillKeyBindingPage(LiteContainer keyBindingPage) { }

    protected virtual void InitializeSystems() { }

    protected virtual void OnLanguageChanged() { }

    protected virtual void OnResetSettings() { }
    protected virtual void SetDefaultTab() => SetSelectedTab(General);

    public sealed override void Awake() {
        base.Awake();

        size = FixedSize;

        SetTheme(ThemeKind.ClassicDark);

        _domain = GetDomain();
        _logger = _domain.ModSystem.Logger;
        _modSystem = _domain.ModSystem;
        _localizationManager = _modSystem.ModLocalizationSystem;
        _settingsSystem = _modSystem.ModSettingsSystem;
        InitializeSystems();

        _tabBar = AddUIComponent<TabBar>();
        _tabBar.size = new Vector2(SectionWidth, 30);
        _tabBar.SetTheme(ThemeKind.ClassicDark);
        _tabBar.relativePosition = new Vector3(BasePadding, BasePadding);
        _tabBar.TabBarItemSetup += SetupTabBarItem;

        AddGeneralTab();
        AddTabs();
        AddAdvancedTab();

        SetDefaultTab();
    }

    protected void AddDebugTab() {
        _debugPageContainer = AddTab(LocalizationStrings.Debug, Localize(LocalizationStrings.Debug));

        FillDebugPage(_debugPageContainer);
    }

    protected SettingsSection AddSection(LiteContainer container, string header = null, string description = null) {
        var section = container.AddUIComponent<SettingsSection>();
        section.SuspendAutoLayout();
        section.width = SectionWidth;
        section.Theme = _theme;
        section.DescriptionScale = 0.8f;

        if (string.IsNullOrEmpty(header)) {
            section.ResumeAutoLayout();
            return section;
        }

        section.Header = header;
        if (string.IsNullOrEmpty(description)) {
            section.ResumeAutoLayout();
            return section;
        }

        section.Description = description;
        section.ResumeAutoLayout();
        return section;
    }

    protected LiteContainer AddTab(string id, string tabDisplayName) {
        var container = AddPage();
        _tabBar.AddTab(id, tabDisplayName, container.parent);
        return container;
    }

    protected string Localize(string key) => _localizationManager.GetString(key);

    protected void SetSelectedTab(string id) => _tabBar?.SetSelectionWithoutNotify(id);

    private void AddAdvancedTab() {
        _advancedPageContainer = AddTab(LocalizationStrings.Advanced, Localize(LocalizationStrings.Advanced));
        var section = AddSection(_advancedPageContainer);

        section.AddButton(_localizationManager.GetString(LocalizationStrings.ChangelogHeader), null, _localizationManager.GetString(LocalizationStrings.Changelog), () => ChangelogDialog.ShowChangelogDialog(_domain));

        section.AddButton(_localizationManager.GetString(LocalizationStrings.CompatibilityCheckHeader), _localizationManager.GetString(LocalizationStrings.CompatibilityCheckDescription), _localizationManager.GetString(LocalizationStrings.Check), () => _modSystem.CompatibilitySystem.CheckAndShowDialog());

        section.AddButton(_localizationManager.GetString(LocalizationStrings.ResetModMajor), _localizationManager.GetString(LocalizationStrings.ResetModMinor), _localizationManager.GetString(LocalizationStrings.Reset), ResetSettings);

        section.AddButton(_localizationManager.GetString(LocalizationStrings.CopyLogsToDesktop), null, _localizationManager.GetString(LocalizationStrings.Copy), CopyLogsToDesktop);

        var logLevelDropdown = section.AddDropdown<LogLevel>(_localizationManager.GetString(LocalizationStrings.LogLevel), _localizationManager.GetString(LocalizationStrings.LogLevelDescription));
        logLevelDropdown.TriggerButton.width = 200;
        logLevelDropdown.DefaultTextSelector = v => _localizationManager.GetString(v.ToString());
        logLevelDropdown.AddOptions(Logger.LogLevelValues);
        logLevelDropdown.SetSelectedWithoutNotify(v => v.Value == _logger.Level);
        logLevelDropdown.ValueSelectionChanged += v => _logger.Level = v;

        var section1 = AddSection(_advancedPageContainer);
        section1.AddButton(_localizationManager.GetString(LocalizationStrings.ImproveModSharedTranslation), null, _localizationManager.GetString(LocalizationStrings.Website), () => URLHelper.OpenURL(_modSystem.SharedTranslationURL));

        if (!string.IsNullOrEmpty(_modSystem.ModTranslationURL)) section1.AddButton(_localizationManager.GetString(LocalizationStrings.ImproveModTranslation), null, _localizationManager.GetString(LocalizationStrings.Website), () => URLHelper.OpenURL(_modSystem.ModTranslationURL));

        if (!string.IsNullOrEmpty(_modSystem.ModSteamURL)) section1.AddButton(_localizationManager.GetString(LocalizationStrings.SteamModPage), null, _localizationManager.GetString(LocalizationStrings.Website), () => URLHelper.OpenSteamURL(_modSystem.ModSteamURL));

        if (!string.IsNullOrEmpty(_modSystem.DiscordURL)) section1.AddButton(_localizationManager.GetString(LocalizationStrings.DiscordGetHelp), null, _localizationManager.GetString(LocalizationStrings.Website), () => URLHelper.OpenURL(_modSystem.DiscordURL));

        FillAdvancedPage(_advancedPageContainer);
    }

    private void AddGeneralTab() {
        _generalPageContainer = AddTab(General, Localize(LocalizationStrings.General));
        var pageContent = _generalPageContainer;

        var modSystem = _domain.ModSystem;
        var modInfoSection = AddSection(pageContent, Localize(LocalizationStrings.ModInfo));
        var channel = modSystem.Channel switch {
            ReleaseChannel.Beta => Localize(LocalizationStrings.BETA),
            ReleaseChannel.Stable => Localize(LocalizationStrings.STABLE),
            _ => "ALPHA"
        };

        #region ModInfoSection.Version

        var versionTextLabel = modInfoSection.AddTextLabel(Localize(LocalizationStrings.Version), $"{modSystem.ModVersion.ToDisplayString()} {channel}");
        versionTextLabel.SuspendAutoLayout();
        var background = versionTextLabel.Background;
        background.Atlas = DefaultAtlas.Instance;
        background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
        var channelColor = modSystem.Channel switch {
            ReleaseChannel.Beta => UIColorPalette.YellowNormal,
            ReleaseChannel.Stable => UIColorPalette.GreenNormal,
            _ => new Color32(6, 132, 138, 255)
        };
        background.Colors.SetValues(channelColor);
        versionTextLabel.SizeMode = TextSizeMode.FitContent;
        versionTextLabel.TextPadding = new EdgeInsets(4, 4, 4, 4);
        versionTextLabel.TextHorizontalAlignment = UIHorizontalAlignment.Left;
        versionTextLabel.ResumeAutoLayout();
        versionTextLabel.eventDoubleClick += OnVersionTextLabelDoubleClick;

        #endregion

        #region ModInfoSection.Compatibility

        var status = _modSystem.CompatibilitySystem.CurrentStatus.IsOnlyFlag(CompatibilityStatus.Normal);
        if (_modSystem.CompatibilitySystem.ShouldRestartGame)
            status = false;

        var compatibilityTextLabel = modInfoSection.AddTextLabel(_localizationManager.GetString(LocalizationStrings.ModCompatibility), status ? _localizationManager.GetString(LocalizationStrings.Normal) : _localizationManager.GetString(LocalizationStrings.Warning));
        compatibilityTextLabel.SuspendAutoLayout();
        compatibilityTextLabel.Background.Atlas = DefaultAtlas.Instance;
        compatibilityTextLabel.Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
        compatibilityTextLabel.Background.Colors.SetValues(status ? UIColorPalette.GreenColors : UIColorPalette.YellowColors);
        compatibilityTextLabel.InvalidateOnStateChanged = true;
        compatibilityTextLabel.TextPadding = new EdgeInsets(4, 4, 4, 2);
        compatibilityTextLabel.width += 8;
        compatibilityTextLabel.eventClicked += OnCompatibilityTextLabelClicked;
        compatibilityTextLabel.ResumeAutoLayout();

        #endregion

        var isDebugBuild = _modSystem.Channel == ReleaseChannel.Alpha;
        var debugBuildDate = string.Empty;
        if (isDebugBuild) {
            var directory = _modSystem.ModDirectory;
            debugBuildDate = Directory.Exists(directory) ? new DirectoryInfo(directory).CreationTime.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        }

        modInfoSection.AddTextLabel($"{_localizationManager.GetString(LocalizationStrings.ModReleaseDate)}", isDebugBuild ? debugBuildDate : $"{_modSystem.BuildDate:yyyy-MM-dd}");

        #region ModInfoSection.Language

        var hasLocaleProgress = _localizationManager.TryGetCurrentLocaleProgress(out var localeProgress);

        var languageDropdown = modInfoSection.AddDropdown<ValueTuple<string, string>>(_localizationManager.GetString(LocalizationStrings.Language), hasLocaleProgress ? $"{_localizationManager.GetString(LocalizationStrings.TranslationProgress)}: {localeProgress}" : null);
        languageDropdown.CanWheel = false;
        languageDropdown.PopupCreated += (_, listBox) => {
            listBox.Padding = new EdgeInsets(0, 0, 4, 4);
            listBox.MaxFitSize = new Size(listBox.MaxFitSize.Width, 800);
        };
        languageDropdown.DefaultTextSelector = tuple => tuple.Item2;
        languageDropdown.AddOptions(_localizationManager.LanguageOptions);
        languageDropdown.SetSelectedWithoutNotify(dropDownOption => _settingsSystem.MainSettings.LocaleId == dropDownOption.Value.Item1);
        languageDropdown.ValueSelectionChanged += OnLanguageOptionSelectedChanged;

        #endregion

        FillGeneralPage(_generalPageContainer);
    }

    protected virtual void OnVersionTextLabelDoubleClick(UIComponent component, UIMouseEventParameter eventParam) { }

    private LiteContainer AddPage() {
        var scrollViewer = AddUIComponent<ScrollViewer>();
        scrollViewer.SetSizeIfChanged(new Vector2(FixedSize.x - BasePadding, PageSize.y));
        scrollViewer.Orientation = ScrollOrientation.Vertical;
        if (scrollViewer.VerticalScrollBar != null)
            scrollViewer.VerticalScrollBar.ThumbControl.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingsSectionBackground);
        var pageContent = scrollViewer.SetContent<LiteContainer>();
        pageContent.SetWidthIfChanged(PageSize.x);
        pageContent.AutoLayout = true;
        pageContent.AutoFitChildrenVertically = true;
        pageContent.RowGap = 20;
        scrollViewer.relativePosition = new Vector2(BasePadding, BasePadding + 30 + 10);
        return pageContent;
    }

    private void CopyLogsToDesktop() {
        try {
            var folderName = _domain.ModSystem.RowName + "Logs";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var destFolder = Path.Combine(desktopPath, folderName);

            if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

            string gameLogFilePath = null;
            switch (Application.platform) {
                case RuntimePlatform.WindowsPlayer:
                    gameLogFilePath = Path.Combine(Application.dataPath, "output_log.txt");
                    break;

                case RuntimePlatform.OSXPlayer:
                    gameLogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Logs/Unity/Player.log");
                    break;

                default:
                    _logger.Error("Unsupported platform for opening game log file");
                    break;
            }

            if (string.IsNullOrEmpty(gameLogFilePath) || !File.Exists(gameLogFilePath)) {
                _logger.Error("Couldn't find game log file when copying logs to desktop");
            }
            else {
                var destGameLogFilePath = Path.Combine(destFolder, "output_log.txt");
                File.Copy(gameLogFilePath, destGameLogFilePath, true);
            }

            var modLogFilePath = _logger.LogPath;
            if (!string.IsNullOrEmpty(modLogFilePath) && File.Exists(modLogFilePath)) {
                var destModLogFilePath = Path.Combine(destFolder, Path.GetFileName(modLogFilePath));
                File.Copy(modLogFilePath, destModLogFilePath, true);
            }
            else {
                _logger.Error("Couldn't find mod log file");
            }

            _logger.Info($"Copied logs to desktop: {destFolder}");
            MessageDialog.ShowMessageDialog(_domain, _localizationManager.GetString(LocalizationStrings.CopiedLogsSucceeded));
        }
        catch (Exception ex) {
            _logger.Error(ex, "Couldn't copy logs to desktop");
            ExceptionDialog.ShowExceptionDialog(_domain, "Couldn't copy logs to desktop", ex.StackTrace);
        }
    }

    private string GetFontPoolString() => $"DynamicFontRenderer Pool Count: {FontPoolInspector.DynamicFontRendererPoolCount}\nMarkupToken Pool Count: {FontPoolInspector.MarkupTokenPoolCount}\nTextLine Pool Count: {FontPoolInspector.TextLinePoolCount}";

    private string GetListPoolCount() {
        var sb = new StringBuilder();
        sb.AppendLine("ListPool<TextLine> Count: " + ListPool<TextLine>.Count);
        sb.AppendLine("ListPool<MarkupToken> Count: " + ListPool<MarkupToken>.Count);
        return sb.ToString();
    }

    private void OnCompatibilityTextLabelClicked(UIComponent component, UIMouseEventParameter eventParam) => _modSystem.CompatibilitySystem.CheckAndShowDialog();

    private void OnLanguageOptionSelectedChanged(ValueTuple<string, string> arg1) {
        _localizationManager.ApplyLocale(arg1.Item1);
        _settingsSystem.MainSettings.LocaleId = arg1.Item1;
        OptionsPanelSystem?.OnLocaleChanged();
        OnLanguageChanged();
    }

    private void OpenGameLogFile() {
        string path;

        switch (Application.platform) {
            case RuntimePlatform.WindowsPlayer:
                path = Path.Combine(Application.dataPath, "output_log.txt");
                break;

            case RuntimePlatform.OSXPlayer:
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    "Library/Logs/Unity/Player.log");
                break;

            default:
                _logger.Error("Unsupported platform for opening game log file");
                return;
        }

        if (!File.Exists(path)) {
            _logger.Error("Couldn't find game log file: " + path);
            return;
        }

        try {
            Process.Start(new ProcessStartInfo {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception e) {
            _logger.Error("Failed to open game log file: " + e);
        }
    }

    private void OpenLogFile() {
        if (!File.Exists(_logger.LogPath)) {
            _logger.Error("Couldn't find log file");
            return;
        }

        Process.Start(new ProcessStartInfo {
            FileName = _logger.LogPath,
            UseShellExecute = true
        });
    }

    private void OpenModLogsDirectory() {
        var dir = Path.GetDirectoryName(_logger.LogPath);
        if (!Directory.Exists(dir)) {
            _logger.Error("Couldn't find logs directory");
            return;
        }

        Process.Start(new ProcessStartInfo {
            FileName = dir,
            UseShellExecute = true
        });
    }

    private void OpenModSettingFile() {
        var path = _settingsSystem.MainSettingsSource.Path;
        if (!File.Exists(_settingsSystem.MainSettingsSource.Path)) {
            _logger.Error("Couldn't find mod setting file");
            return;
        }

        Process.Start(new ProcessStartInfo {
            FileName = path,
            UseShellExecute = true
        });
    }

    private void OpenSettingsDirectory() {
        var dir = Path.GetDirectoryName(_settingsSystem.MainSettingsSource.Path);
        if (!Directory.Exists(dir)) {
            _logger.Error("Couldn't find settings directory");
            return;
        }

        Process.Start(new ProcessStartInfo {
            FileName = dir,
            UseShellExecute = true
        });
    }

    private void ResetSettings() {
        try {
            ConfirmDialog.ShowConfirmDialog($"{_modSystem.ModName} {_localizationManager.GetString(LocalizationStrings.Reset)}", _localizationManager.GetString(LocalizationStrings.ResetModWarning), _localizationManager.GetString(LocalizationStrings.OK), _localizationManager.GetString(LocalizationStrings.Cancel), ResetSettingsConfirmInvoke, d => d.Close(), d => d.Buttons.First().Background.Colors.SetValues(UIColorPalette.RedColors));
        }
        catch (Exception e) {
            _logger.Error(e, "Reset settings failed");
            MessageDialog.ShowMessageDialog($"{_modSystem.ModName} {_localizationManager.GetString(LocalizationStrings.Reset)}", _localizationManager.GetString(LocalizationStrings.ResetModFailed), _localizationManager.GetString(LocalizationStrings.OK));
        }
    }

    private void ResetSettingsConfirmInvoke(ConfirmDialog confirmDialog) {
        try {
            confirmDialog?.Close();
            _settingsSystem.MainSettings.SetDefaults();
            _localizationManager.ApplyLocale();
            OptionsPanelSystem?.OnLocaleChanged();
            MessageDialog.ShowMessageDialog($"{_modSystem.ModName} {_localizationManager.GetString(LocalizationStrings.Reset)}", _localizationManager.GetString(LocalizationStrings.ResetModSucceeded), _localizationManager.GetString(LocalizationStrings.OK));
            OnResetSettings();
            _logger.Info("Reset mod settings succeeded");
        }
        catch (Exception e) {
            _logger.Error(e, "Reset settings failed");
        }
    }

    private void SetupTabBarItem(TabBarItem tabBarItem) {
        tabBarItem.SetTheme(ThemeKind.ClassicDark);
        tabBarItem.TextPadding = new EdgeInsets(0, 0, 2, 0);
    }
}