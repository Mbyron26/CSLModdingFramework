using System.Linq;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Fonts;

public static class FontManager {
    private static DynamicFont _openSansRegularFont;
    private static DynamicFont _openSansSemibold;
    public static UIFont[] FindFonts() => Resources.FindObjectsOfTypeAll<UIFont>();

    public static DynamicFont OpenSansRegularFont {
        get {
            _openSansRegularFont ??= new DynamicFont((UIDynamicFont)FindFonts().FirstOrDefault(x => x.name == "OpenSans-Regular"));

            return _openSansRegularFont;
        }
    }

    public static DynamicFont OpenSansSemibold {
        get {
            _openSansSemibold ??= new DynamicFont((UIDynamicFont)FindFonts().FirstOrDefault(x => x.name == "OpenSans-Semibold"));

            return _openSansSemibold;
        }
    }
}