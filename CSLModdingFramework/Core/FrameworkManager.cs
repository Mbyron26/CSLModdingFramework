using System;
using System.Reflection;
using ColossalFramework.UI;

namespace CSLModdingFramework.Core;

public static class FrameworkManager {
    public static readonly Version CurrentVersion;

    static FrameworkManager() => CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
}