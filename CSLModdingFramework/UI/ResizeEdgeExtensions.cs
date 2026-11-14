using CSLModdingFramework.UI.Controls;

namespace CSLModdingFramework.UI;

public static class ResizeEdgeExtensions {
    public static bool IsFlagSet(this ResizeHandle.ResizeEdge value, ResizeHandle.ResizeEdge flag) => flag == (value & flag);

    public static ResizeHandle.ResizeEdge SetFlag(this ResizeHandle.ResizeEdge value, ResizeHandle.ResizeEdge flag, bool on) {
        if (on) {
            return value | flag;
        }

        return value & ~flag;
    }
}