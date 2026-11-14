namespace CSLModdingFramework.Core.Compatibility;

public readonly struct GameVersionCompatibility {
    public GameVersion MinGameVersion { get; }
    public GameVersion MaxGameVersion { get; }

    public GameVersionCompatibility(string gameVersion)
        : this(new GameVersion(gameVersion), new GameVersion(gameVersion)) { }

    public GameVersionCompatibility(GameVersion gameVersion)
        : this(gameVersion, gameVersion) { }

    public GameVersionCompatibility(string minVersion, string maxVersion)
        : this(new GameVersion(minVersion), new GameVersion(maxVersion)) { }

    public GameVersionCompatibility(int gameVersionMajor, int gameVersionMinor, int gameVersionPatch, int gameVersionBuild = 0)
        : this(new GameVersion(gameVersionMajor, gameVersionMinor, gameVersionPatch, gameVersionBuild)) { }

    public GameVersionCompatibility(GameVersion minGameVersion, GameVersion maxGameVersion) {
        MinGameVersion = minGameVersion;
        MaxGameVersion = maxGameVersion;
    }

    private static int CompareIgnoringBuild(GameVersion left, GameVersion right) {
        var cmp = left.Major.CompareTo(right.Major);
        if (cmp != 0) return cmp;
        cmp = left.Minor.CompareTo(right.Minor);
        if (cmp != 0) return cmp;
        return left.Patch.CompareTo(right.Patch);
    }

    public override string ToString() => MinGameVersion.Equals(MaxGameVersion)
        ? $"{MinGameVersion}"
        : $"{MinGameVersion} - {MaxGameVersion}";

    public bool IsCompatible(GameVersion gameVersion) {
        var ignoreBuild = MinGameVersion.Build == 0 && MaxGameVersion.Build == 0;

        if (ignoreBuild) {
            var withinMin = CompareIgnoringBuild(gameVersion, MinGameVersion) >= 0;
            var withinMax = CompareIgnoringBuild(gameVersion, MaxGameVersion) <= 0;
            return withinMin && withinMax;
        }

        return gameVersion >= MinGameVersion && gameVersion <= MaxGameVersion;
    }
}