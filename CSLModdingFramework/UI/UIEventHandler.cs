using ColossalFramework.UI;

namespace CSLModdingFramework.UI;

public delegate void UIEventHandler<TSender>(TSender sender) where TSender : UIComponent;

public delegate void UIEventHandler<TSender, TValue>(TSender sender, TValue value) where TSender : UIComponent;