using System;
using System.Collections.Generic;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;

namespace CSLModdingFramework.Core;

public class ChangelogVersion {
    private readonly List<ChangelogEntry> _entries = new();
    private IReadOnlyList<ChangelogEntry> _readOnlyList;

    public Version Version { get; }
    public DateTime Date { get; }
    public IReadOnlyList<ChangelogEntry> Entries => _readOnlyList ??= _entries.AsReadOnlyList();

    public ChangelogVersion(Version version, DateTime date) {
        Version = version;
        Date = date;
    }

    public ChangelogVersion AddEntry(ChangelogType type, FormattedString description) {
        _entries.Add(new ChangelogEntry(type, description));
        return this;
    }
}