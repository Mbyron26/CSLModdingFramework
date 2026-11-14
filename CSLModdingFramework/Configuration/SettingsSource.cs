namespace CSLModdingFramework.Configuration;

public class SettingsSource {
    public SettingsSource(string path, ISettings settings) {
        Path = path;
        Settings = settings;
    }

    public string Path { get; set; }
    public ISettings Settings { get; set; }

    public override string ToString() => Path;
}