using ColossalFramework.UI;

namespace CSLModdingFramework.UI;

public static class UIViewExtensions {
    public static T AddUIComponent<T>(this UIView view) where T : UIComponent => view.AddUIComponent(typeof(T)) as T;
}