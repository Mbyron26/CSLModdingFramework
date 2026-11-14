using System.Collections.Generic;

namespace CSLModdingFramework.Localization;

public interface ILocalizationSource {
    Dictionary<string, LocalizationDictionary> LocaleDictionaries { get; }
    List<LocalizationResourceInfo> ResourceInfos { get; }
    void Load();
}