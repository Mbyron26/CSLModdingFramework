using CSLModdingFramework.Configuration;

namespace CSLModdingFramework.ToolPanel;

public class ToolButtonModSettingsBase : ModSettingsBase {
    public ToolButtonScope ToolButtonDisplayScope { get; set; } = ToolButtonScope.UnifiedUI;
    public float ToolButtonPositionX { get; set; }
    public float ToolButtonPositionY { get; set; }
}