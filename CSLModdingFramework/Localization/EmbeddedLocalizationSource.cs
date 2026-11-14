using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Logging;
using CSLModdingFramework.Utilities;

namespace CSLModdingFramework.Localization;

public class EmbeddedLocalizationSource {
    public const string JsonExtension = ".json";
    private readonly Assembly _assembly;
    private readonly string _prefix;
    private readonly string[] _resourceNames;
    private readonly Dictionary<string, LocalizationDictionary> _localeDictionaries;
    private List<LocalizationResourceInfo> _translationStatuses;

    public IReadOnlyDictionary<string, LocalizationDictionary> LocaleDictionaries { get; }
    public IReadOnlyList<LocalizationResourceInfo> TranslationStatuses { get; private set; }

    public EmbeddedLocalizationSource(Assembly assembly, string prefix) {
        _localeDictionaries = new Dictionary<string, LocalizationDictionary>();
        LocaleDictionaries = _localeDictionaries.AsReadOnlyDictionary();

        _translationStatuses = new List<LocalizationResourceInfo>();

        _assembly = assembly;
        _prefix = prefix;
        _resourceNames = _assembly.GetManifestResourceNames();
    }

    public void Load() {
        foreach (var res in _resourceNames) {
            if (!res.StartsWith(_prefix)) continue;
            if (!res.EndsWith(JsonExtension)) continue;

            try {
                var stripped = res.Substring(_prefix.Length + 1);
                var parts = stripped.Split('.');
                if (parts.Length < 2) continue;

                var locale = parts[0];

                using var stream = _assembly.GetManifestResourceStream(res);
                if (stream != null) {
                    using var reader = new StreamReader(stream);
                    var jsonText = reader.ReadToEnd();

                    if (locale == "TranslationStatus") {
                        _translationStatuses = JsonHelper.Deserialize<List<LocalizationResourceInfo>>(jsonText);
                        TranslationStatuses = _translationStatuses.AsReadOnlyList();
                        continue;
                    }

                    if (_localeDictionaries.ContainsKey(locale)) continue;

                    var dic = JsonHelper.Deserialize<Dictionary<string, string>>(jsonText);
                    var localizationDictionary = new LocalizationDictionary(locale);

                    foreach (var kv in dic) {
                        localizationDictionary.Add(kv.Key, kv.Value);
                    }

                    _localeDictionaries[locale] = localizationDictionary;
                }
            }
            catch (Exception e) {
                var assemblyName = _assembly.GetName().Name;
                LogManager.GetLogger().Error(e, $"Failed to load embedded localization resource from assembly '{assemblyName}', resource '{res}'");
            }
        }
    }
}