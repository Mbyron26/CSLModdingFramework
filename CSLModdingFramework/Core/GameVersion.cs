using System;

namespace CSLModdingFramework.Core;

public struct GameVersion : IComparable<GameVersion>, IEquatable<GameVersion> {
    private string _cachedToString;

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public int Build { get; }

    public GameVersion(int major, int minor, int patch, int build = 0) {
        Major = major;
        Minor = minor;
        Patch = patch;
        Build = build;
    }

    public GameVersion(string versionString) {
        if (string.IsNullOrEmpty(versionString))
            throw new ArgumentNullException(nameof(versionString));

        var parts = versionString.Split('-');
        var nums = parts[0].Split('.');
        if (nums.Length < 3)
            throw new FormatException($"Invalid game version: {versionString}");

        Major = int.Parse(nums[0]);
        Minor = int.Parse(nums[1]);
        Patch = int.Parse(nums[2]);

        if (parts.Length > 1 && parts[1].StartsWith("f"))
            Build = int.TryParse(parts[1].Substring(1), out var f) ? f : 0;
        else
            Build = 0;
    }

    public readonly override bool Equals(object obj) => obj is GameVersion other && Equals(other);

    public override string ToString() {
        _cachedToString ??= Build > 0
            ? $"{Major}.{Minor}.{Patch}-f{Build}"
            : $"{Major}.{Minor}.{Patch}";
        return _cachedToString;
    }

    public readonly override int GetHashCode() {
        unchecked {
            var hash = 17;
            hash = hash * 31 + Major.GetHashCode();
            hash = hash * 31 + Minor.GetHashCode();
            hash = hash * 31 + Patch.GetHashCode();
            hash = hash * 31 + Build.GetHashCode();
            return hash;
        }
    }

    public static bool operator <(GameVersion left, GameVersion right) => left.CompareTo(right) < 0;

    public static bool operator >(GameVersion left, GameVersion right) => left.CompareTo(right) > 0;

    public static bool operator <=(GameVersion left, GameVersion right) => left.CompareTo(right) <= 0;

    public static bool operator >=(GameVersion left, GameVersion right) => left.CompareTo(right) >= 0;

    public readonly int CompareTo(GameVersion other) {
        var cmp = Major.CompareTo(other.Major);
        if (cmp != 0) return cmp;
        cmp = Minor.CompareTo(other.Minor);
        if (cmp != 0) return cmp;
        cmp = Patch.CompareTo(other.Patch);
        return cmp != 0 ? cmp : Build.CompareTo(other.Build);
    }

    public readonly bool Equals(GameVersion other) => CompareTo(other) == 0;
}