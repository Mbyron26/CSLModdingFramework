using System.Collections.Generic;
using System.Linq;
using ColossalFramework.Plugins;
using ICities;

namespace CSLModdingFramework.Utilities;

public static class PluginHelper {
    public static IEnumerable<PluginManager.PluginInfo> GetPluginsInfo() => PluginManager.instance.GetPluginsInfo();

    public static PluginManager.PluginInfo FindPluginByAssemblyName(string assemblyName) {
        foreach (var plugin in GetPluginsInfo()) {
            if (ContainsAssembly(plugin, assemblyName))
                return plugin;
        }

        return null;
    }

    public static bool IsPluginEnabled(string assemblyName) {
        var plugin = FindPluginByAssemblyName(assemblyName);
        return plugin != null && plugin.isEnabled;
    }

    public static bool IsPluginEnabled(ulong id) {
        foreach (var plugin in GetPluginsInfo()) {
            if (plugin.isEnabled &&
                plugin.publishedFileID.AsUInt64 == id) {
                return true;
            }
        }

        return false;
    }

    public static bool IsPluginSubscribed(string assemblyName) => FindPluginByAssemblyName(assemblyName) != null;

    public static string GetPluginDirectory(string assemblyName) => FindPluginByAssemblyName(assemblyName)?.modPath;

    public static void GetPluginState(string assemblyName, out bool isSubscribed, out bool isEnabled) {
        var plugin = FindPluginByAssemblyName(assemblyName);

        isSubscribed = plugin != null;
        isEnabled = plugin is { isEnabled: true };
    }

    public static IEnumerable<PluginManager.PluginInfo> GetPluginsInfoSortedByName() => GetPluginsInfo().Where(p => p?.userModInstance is IUserMod).OrderBy(p => ((IUserMod)p.userModInstance).Name);

    private static bool ContainsAssembly(PluginManager.PluginInfo plugin, string assemblyName) => plugin.GetAssemblies().Any(a => string.Equals(a.GetName().Name, assemblyName));
}