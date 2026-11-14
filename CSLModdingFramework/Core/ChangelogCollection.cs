using System;
using System.Collections.Generic;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Localization;

namespace CSLModdingFramework.Core;

public class ChangelogCollection {
    public static readonly ChangelogType[] ChangelogTypes = {
        ChangelogType.Added,
        ChangelogType.Removed,
        ChangelogType.Updated,
        ChangelogType.Fixed,
        ChangelogType.Optimized,
        ChangelogType.Translation,
        ChangelogType.Attention
    };

    private readonly List<ChangelogVersion> _versions = new();
    private IReadOnlyList<ChangelogVersion> _readOnlyList;
    private readonly ILocalizationProvider _localizationProvider;

    public IReadOnlyList<ChangelogVersion> Versions => _readOnlyList ??= _versions.AsReadOnlyList();

    public ChangelogCollection(ILocalizationProvider localizationProvider = null) => _localizationProvider = localizationProvider;

    public ChangelogVersion Add(Version version, DateTime date, bool autoGenerate = true) {
        if (version is null) return null;

        var changelogVersion = new ChangelogVersion(version, date);
        if (autoGenerate && _localizationProvider is not null) {
            var prefixKey = GetPrefixKey(version);

            foreach (var key in _localizationProvider.GetKeys(prefixKey)) {
                var type = GetChangelogType(prefixKey, key);

                if (type == ChangelogType.None) continue;

                changelogVersion.AddEntry(type, key);
            }
        }

        _versions.Add(changelogVersion);
        return changelogVersion;
    }

    private static ChangelogType GetChangelogType(string prefix, string key) {
        foreach (var type in ChangelogTypes) {
            if (key.Contains($"{prefix}_{type}"))
                return type;
        }

        return ChangelogType.None;
    }

    private static string GetPrefixKey(Version version) => version.Revision >= 0
        ? $"Changelog_v{version.Major}_{version.Minor}_{version.Build}_{version.Revision}"
        : $"Changelog_v{version.Major}_{version.Minor}_{version.Build}";
}