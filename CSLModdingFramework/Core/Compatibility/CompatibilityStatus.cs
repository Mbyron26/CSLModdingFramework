using System;

namespace CSLModdingFramework.Core.Compatibility;

[Flags]
public enum CompatibilityStatus {
    Normal = 1 << 0,
    MissingDependencies = 1 << 1,
    IncompatibleMods = 1 << 2,
    NotMakeForCurrentGameVersion = 1 << 3,
    UnknownError = 1 << 4
}