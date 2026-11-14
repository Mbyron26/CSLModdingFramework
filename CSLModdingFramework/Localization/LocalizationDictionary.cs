using System;
using System.Collections.Generic;

namespace CSLModdingFramework.Localization;

public class LocalizationDictionary {
    private readonly Dictionary<string, string> _entries;

    public string LocaleCode { get; }
    public int Count => _entries.Count;
    public IEnumerable<string> LocaleKeys => _entries.Keys;
    public IEnumerable<KeyValuePair<string, string>> Entries => _entries;

    public LocalizationDictionary(string localeCode) {
        LocaleCode = localeCode ?? throw new ArgumentNullException(nameof(localeCode));
        _entries = new Dictionary<string, string>();
    }

    public bool TryGetValue(string localeKey, out string value) => localeKey == null ? throw new ArgumentNullException(nameof(localeKey)) : _entries.TryGetValue(localeKey, out value);

    public IEnumerable<string> GetKeys(string prefix) {
        if (string.IsNullOrEmpty(prefix))
            yield break;

        foreach (var key in _entries.Keys) {
            if (key.StartsWith(prefix, StringComparison.Ordinal))
                yield return key;
        }
    }

    public void Add(string localeKey, string value) {
        if (localeKey == null)
            throw new ArgumentNullException(nameof(localeKey));

        _entries[localeKey] = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void Clear() => _entries.Clear();

    public bool Contains(string key) => key != null && _entries.ContainsKey(key);

    public void MergeFrom(LocalizationDictionary other, bool overwrite = true) {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        foreach (var kv in other._entries) {
            if (overwrite || !_entries.ContainsKey(kv.Key)) {
                _entries[kv.Key] = kv.Value;
            }
        }
    }
}