using System;
using System.IO;

namespace CSLModdingFramework.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public class SettingsFileAttribute : Attribute {
    public SettingsFileAttribute(string fileName) => FileName = Path.ChangeExtension(fileName, ".json");

    private SettingsFileAttribute() { }
    public string FileName { get; private set; }
}