using System.Collections.Generic;
using CSLModdingFramework.Core;

namespace CSLModdingFramework.Localization;

public interface ILocalizationProvider {
    string GetString(string key);
    string GetString(string key, params object[] args);
    string GetString(FormattedString formattedString);
    bool TryGetString(string key, out string value);
    IEnumerable<string> GetKeys(string prefix);
}