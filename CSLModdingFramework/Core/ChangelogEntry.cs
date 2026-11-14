namespace CSLModdingFramework.Core;

public class ChangelogEntry {
    public ChangelogType Kind { get; }
    public FormattedString Description { get; }

    public ChangelogEntry(ChangelogType kind, FormattedString description) {
        Kind = kind;
        Description = description;
    }

    public override string ToString() => $"{Kind}: {Description}";
}