using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class ScrollableCheckboxListBox : ScrollViewer {
    public override ScrollOrientation Orientation {
        get => _orientation;
        set { }
    }

    public override ScrollSizePolicy SizeMode {
        get => ScrollSizePolicy.Fixed;
        set { }
    }

    public CheckboxList List { get; private set; }

    public override void Awake() {
        base.Awake();

        SuspendAutoLayout();

        SetOrientation(ScrollOrientation.Vertical);
        Padding = new EdgeInsets(0, 0, 4, 4);
        Orientation = ScrollOrientation.Vertical;
        VerticalScrollBarMargin = new EdgeInsets(0, 0, 4, 4);
        
        List = SetContent<CheckboxList>();
        List.Padding = new EdgeInsets(4, 4, 0, 0);
        List.size = size;

        Background.Atlas = DefaultAtlas.Instance;
        Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
        Background.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlBackground);
        if (VerticalScrollBar != null) {
            VerticalScrollBar.Background.Atlas = DefaultAtlas.Instance;
            VerticalScrollBar.Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius4);
            VerticalScrollBar.Background.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlBackgroundHovered);
            VerticalScrollBar.ThumbControl.Colors.SetValues(new Color32(96, 110, 130, 255));
        }

        ResumeAutoLayout();
    }
}