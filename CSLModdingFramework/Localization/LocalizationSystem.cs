using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Globalization;
using CSLModdingFramework.Configuration;
using CSLModdingFramework.Core;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Logging;
using CSLModdingFramework.Utilities;

namespace CSLModdingFramework.Localization;

public sealed class LocalizationSystem : SystemBase, ILocalizationProvider {
    public const string UseGameLanguage = "UGL";
    public const string ModDescriptionLocalizedId = "ModDescription";

    private static readonly ILog InternalLogger = LogManager.GetLogger();
    private static EmbeddedLocalizationSource CommonLocalizationSource;

    private ModSettingsBase _modSettings;
    private FileLocalizationSource _modLocalizationSource;
    private LocalizationDictionary _fallbackDictionary;
    private LocalizationDictionary _currentDictionary;
    private bool _processing;

    public event Action<string> LocaleChanged;

    public string GameActiveLocale => LocaleManager.exists ? NormalizeGameLocale(LocaleManager.instance.language) : NormalizeGameLocale(new SavedString(Settings.localeID, Settings.gameSettingsFile, DefaultSettings.localeID).value);
    public ValueTuple<string, string>[] LanguageOptions { get; private set; }

    static LocalizationSystem() => LoadEmbeddedCommonLocaleSource();

    protected override void OnCreate() {
        base.OnCreate();

        _modSettings = Domain.GetOrCreateSystem<SettingsSystem>().MainSettings;
        _modLocalizationSource = new FileLocalizationSource(Path.Combine(Domain.ModSystem.ModDirectory, "Localization"), Logger);

        LoadModSources();
        ApplyLocale();
        LocaleManager.eventLocaleChanged += OnGameLocaleChanged;
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        LocaleManager.eventLocaleChanged -= OnGameLocaleChanged;
    }

    private static void LoadEmbeddedCommonLocaleSource() {
        // using var pc = PerformanceCounter.Start(v => LogManager.GetLogger().Info($"LoadEmbeddedCommonLocaleSource cost: {v.Milliseconds}"));
        CommonLocalizationSource = new EmbeddedLocalizationSource(Assembly.GetExecutingAssembly(), "CSLModdingFramework.Localization.Common");
        CommonLocalizationSource.Load();

        var locales = string.Join(" ", CommonLocalizationSource.LocaleDictionaries.Keys.ToArray());
        InternalLogger.Info($"Load common embedded locale source: {locales}");
    }

    public string GetString(string localeId, string key) {
        if (key is null)
            return string.Empty;

        if (TryGetString(localeId, key, out var value))
            return value;

        Logger.Debug($"Cannot find [{key}] in locale [{localeId}], fallback key");
        return key;
    }

    public bool TryGetString(string localeId, string key, out string value) {
        value = string.Empty;

        if (localeId is null || key is null)
            return false;

        return _modLocalizationSource.LocaleDictionaries.TryGetValue(localeId, out var dictionary) && dictionary.TryGetValue(key, out value);
    }

    public string GetString(string key) {
        if (key is null)
            return string.Empty;

        if (TryGetString(key, out var value))
            return value;

        Logger.Debug($"Cannot find [{key}] in LocalizationDictionary, fallback key");
        return key;
    }

    public bool TryGetString(string key, out string value) {
        if (key is not null && _currentDictionary is not null)
            return _currentDictionary.TryGetValue(key, out value);

        value = string.Empty;
        return false;
    }

    public string GetString(string key, params object[] args) {
        var value = GetString(key);

        if (args == null || args.Length == 0)
            return value;

        return string.Format(value, args);
    }

    public string GetString(FormattedString formattedString) {
        var text = GetString(formattedString.Key);
        return formattedString.Args is null || formattedString.Args.Length == 0 ? text : string.Format(text, formattedString.Args);
    }

    public IEnumerable<string> GetKeys(string prefix) => _currentDictionary?.GetKeys(prefix) ?? Enumerable.Empty<string>();

    public bool TryGetCurrentLocaleProgress(out string progress) {
        progress = null;

        if (_currentDictionary == null) return false;

        if (ReferenceEquals(_currentDictionary, _fallbackDictionary)) {
            progress = "100%";
            return true;
        }

        var localeCode = _currentDictionary.LocaleCode;

        var infos = _modLocalizationSource.ResourceInfos;
        if (infos == null || infos.Count == 0)
            return false;

        var info = infos.Find(v => v.NormalizedLanguageCode == localeCode);
        if (info == null || info.TotalStrings == 0)
            return false;

        var percentage = (float)info.TranslatedStrings / info.TotalStrings;
        progress = $"{(int)(percentage * 100)}%";
        return true;
    }

    public void ApplyLocale(string localeCode = null, Action action = null) {
        if (_processing) return;

        _processing = true;

        var targetLocale = localeCode ?? _modSettings.LocaleId;

        if (targetLocale == UseGameLanguage) {
            _currentDictionary = _modLocalizationSource.LocaleDictionaries.TryGetValue(GameActiveLocale, out var localizationDictionary) ? localizationDictionary : _fallbackDictionary;
        }
        else {
            _currentDictionary = _modLocalizationSource.LocaleDictionaries.TryGetValue(targetLocale, out var localizationDictionary) ? localizationDictionary : _fallbackDictionary;
        }

        Logger.Verbose($"Apply locale: {_currentDictionary.LocaleCode}, game active locale: {GameActiveLocale}, source={(targetLocale == UseGameLanguage ? "Game" : "Custom")}");

        LanguageOptions = GetLanguageOptions();
        action?.Invoke();
        NotifyLocaleChanged();
        _processing = false;
    }

    private void OnGameLocaleChanged() {
        if (_processing) return;

        ApplyLocale();
    }

    private ValueTuple<string, string>[] GetLanguageOptions() {
        if (_modLocalizationSource is null) return ArrayExtensions.Empty<ValueTuple<string, string>>();

        var localeDictionaries = _modLocalizationSource.LocaleDictionaries;
        var valueTuple = new ValueTuple<string, string>[localeDictionaries.Count + 1];

        valueTuple[0] = new ValueTuple<string, string>(UseGameLanguage, GetString(LocalizationStrings.UseGameLanguage));

        var i = 1;
        foreach (var keyValuePair in localeDictionaries) {
            var localeId = keyValuePair.Key;
            var dic = keyValuePair.Value;
            string suffix;
            if (dic.TryGetValue($"Language_{localeId}", out var language)) {
                suffix = $"({language})";
            }
            else {
                suffix = GetString("en-US", $"Language_{localeId}");
            }

            var prefix = GetString($"Language_{localeId}");
            var total = prefix + suffix;
            var display = _currentDictionary.LocaleCode == localeId ? prefix : total;
            valueTuple[i] = new ValueTuple<string, string>(localeId, display);
            i++;
        }

        return valueTuple;
    }

    private void NotifyLocaleChanged() {
        LocaleChanged?.Invoke(_currentDictionary.LocaleCode);
    }

    private void LoadModSources() {
        using var pc = PerformanceCounter.Start(c => Logger.Verbose($"Load mod sources cost: {c.Milliseconds}"));

        _modLocalizationSource.Load();

        var modLocaleDictionaries = _modLocalizationSource.LocaleDictionaries;

        if (modLocaleDictionaries.Count == 0) {
            Logger.Debug("No localization files found for this mod. Using framework default localization.");

            foreach (var (localeCode, commonDic) in CommonLocalizationSource.LocaleDictionaries) {
                var newDic = new LocalizationDictionary(localeCode);
                newDic.MergeFrom(commonDic);
                modLocaleDictionaries[localeCode] = newDic;
            }

            _fallbackDictionary = modLocaleDictionaries[Languages.FallbackLanguage];
            _currentDictionary = _fallbackDictionary;
            return;
        }

        foreach (var (localeCode, modDic) in modLocaleDictionaries) {
            if (CommonLocalizationSource.LocaleDictionaries.TryGetValue(localeCode, out var commonDic)) {
                modDic.MergeFrom(commonDic);
            }
        }

        _fallbackDictionary = modLocaleDictionaries.TryGetValue(Languages.FallbackLanguage, out var enDic) ? enDic : modLocaleDictionaries.First().Value;

        foreach (var (localeCode, modDic) in modLocaleDictionaries) {
            if (ReferenceEquals(modDic, _fallbackDictionary))
                continue;

            foreach (var (key, value) in _fallbackDictionary.Entries) {
                if (modDic.Contains(key)) continue;

                Logger.Warn($"Missing key '{key}' in locale '{localeCode}', using fallback value.");
                modDic.Add(key, value);
            }
        }

        _currentDictionary = _fallbackDictionary;
        Logger.Info($"Loaded {_modLocalizationSource.LocaleDictionaries.Count} locale dictionaries: " + _modLocalizationSource.LocaleDictionaries.Keys.ToArray().Join(", "));
    }

    private string NormalizeGameLocale(string locale) => locale switch {
        "de" => "de-DE",
        "en" => "en-US",
        "es" => "es-ES",
        "fr" => "fr-FR",
        "ko" => "ko-KR",
        "pl" => "pl-PL",
        "pt" => "pt-BR",
        "ru" => "ru-RU",
        "zh" => "zh-CN",
        _ => locale
    };
}