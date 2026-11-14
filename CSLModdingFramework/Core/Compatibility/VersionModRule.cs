using System.Collections.Generic;
using ColossalFramework.Plugins;
using CSLModdingFramework.Extensions;

namespace CSLModdingFramework.Core.Compatibility;

public class VersionModRule : IModRule, IVersionModRule {
    public static GameVersion Current => new(BuildConfig.applicationVersion);

    public GameVersionCompatibility VersionCompatibility { get; private set; }
    public bool IsMatched { get; private set; }

    public void Set(GameVersionCompatibility item) => VersionCompatibility = item;

    public void Set(int gameVersionMajor, int gameVersionMinor, int gameVersionPatch, int gameVersionBuild = 0) => Set(new GameVersionCompatibility(gameVersionMajor, gameVersionMinor, gameVersionPatch, gameVersionBuild));

    public void Check(IEnumerable<PluginManager.PluginInfo> pluginsInfo, ref CompatibilityStatus status) {
        if (!VersionCompatibility.IsCompatible(Current)) {
            status = status.RemoveStatus(CompatibilityStatus.Normal).AddStatus(CompatibilityStatus.NotMakeForCurrentGameVersion);
            IsMatched = true;
        }
        else {
            IsMatched = false;
        }
    }
}