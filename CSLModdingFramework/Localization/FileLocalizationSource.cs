using System;
using System.Collections.Generic;
using System.IO;
using CSLModdingFramework.Logging;
using CSLModdingFramework.Utilities;

namespace CSLModdingFramework.Localization;

public class FileLocalizationSource : ILocalizationSource {
    private readonly string _directory;
    private readonly ILog _logger;

    public Dictionary<string, LocalizationDictionary> LocaleDictionaries { get; }
    public List<LocalizationResourceInfo> ResourceInfos { get; private set; }

    public FileLocalizationSource(string directory, ILog logger) {
        _directory = directory;
        _logger = logger;
        LocaleDictionaries = new Dictionary<string, LocalizationDictionary>();
        ResourceInfos = new List<LocalizationResourceInfo>();
    }

    public void Load() {
        if (!Directory.Exists(_directory)) {
            _logger.Debug($"Directory {_directory} does not exist when loading file localization source.");
            return;
        }

        try {
            foreach (var file in Directory.GetFiles(_directory, "*.json")) {
                var locale = Path.GetFileNameWithoutExtension(file);
                var json = File.ReadAllText(file);

                if (locale == "TranslationStatus") {
                    ResourceInfos = JsonHelper.Deserialize<List<LocalizationResourceInfo>>(json);
                    continue;
                }

                var dic = JsonHelper.Deserialize<Dictionary<string, string>>(json);
                var localizationDictionary = new LocalizationDictionary(locale);

                foreach (var kv in dic)
                    localizationDictionary.Add(kv.Key, kv.Value);

                LocaleDictionaries[locale] = localizationDictionary;
            }
        }
        catch (Exception e) {
            _logger.Error(e, $"Failed to load file localization source from {_directory}");
        }
    }
}