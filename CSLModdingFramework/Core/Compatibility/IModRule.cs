using System.Collections.Generic;
using ColossalFramework.Plugins;

namespace CSLModdingFramework.Core.Compatibility;

public interface IModRule {
    bool IsMatched { get; }
    void Check(IEnumerable<PluginManager.PluginInfo> pluginsInfo, ref CompatibilityStatus status);
}