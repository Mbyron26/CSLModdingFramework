namespace CSLModdingFramework.Core.Compatibility;

public class DependencyModItem {
    public string AssemblyName { get; internal set; }
    public bool IsIncluded { get; internal set; }
    public bool IsEnabled { get; internal set; }
    public string DisplayName { get; internal set; }

    public DependencyModItem(string assemblyName, string displayName) {
        AssemblyName = assemblyName;
        DisplayName = displayName;
    }
}