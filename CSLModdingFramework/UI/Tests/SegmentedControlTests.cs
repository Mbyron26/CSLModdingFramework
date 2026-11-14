using CSLModdingFramework.UI.Controls;
using UnityEngine;

namespace CSLModdingFramework.UI.Tests;

public class SegmentedControlTests {
    private static SegmentedControl<SegmentedControlClassTest> _segmentedControl;

    public static SegmentedControlClassTest SegmentedControlClassTestValue0 { get; set; } = new() {
        Icon = DefaultAtlas.Copy,
        Text = "Text 0",
        Data = new Vector2(0, 0)
    };

    public static SegmentedControlClassTest SegmentedControlClassTestValue1 { get; set; } = new() {
        Icon = DefaultAtlas.Paste,
        Text = "Text 1",
        Data = new Vector2(1, 1)
    };

    public static SegmentedControlClassTest SegmentedControlClassTestValue2 { get; set; } = new() {
        Icon = DefaultAtlas.Clean,
        Text = "Text 2",
        Data = new Vector2(2, 2)
    };

    public static SegmentedControlClassTest SegmentedControlClassTestValue3 { get; set; } = new() {
        Icon = DefaultAtlas.ResetOutline,
        Text = "Text 3",
        Data = new Vector2(3, 3)
    };

    public static Vector2 SelectedData { get; set; } = new(3, 3);

    public static SegmentedControl<SegmentedControlClassTest> SegmentedControl {
        get {
            if (_segmentedControl is null) {
                _segmentedControl = new SegmentedControl<SegmentedControlClassTest>();
                _segmentedControl.SegmentItemCreated += item => {
                    item.IconAtlas = DefaultAtlas.Instance;
                    item.SetTheme(ThemeKind.ClassicDark);
                    item.IconSize = new Vector2(32, 32);
                    item.ContentKind = SegmentItemContentKind.IconText;
                };
                _segmentedControl.DefaultIconNameSelector = value => value.Icon;
                _segmentedControl.DefaultTextSelector = value => value.Text;
                _segmentedControl.AddOptions(SegmentedControlClassTestValue0, SegmentedControlClassTestValue1, SegmentedControlClassTestValue2, SegmentedControlClassTestValue3);

                _segmentedControl.SetSelectedWithoutNotify(value => value.Value.Data == SelectedData);
                _segmentedControl.ValueSelectionChanged += option => {
                    SelectedData = option.Data;
                    Debug.Log($"Selected: {option.Data}");
                };
            }

            return _segmentedControl;
        }
    }

    public class SegmentedControlClassTest {
        public string Icon { get; set; }
        public string Text { get; set; }
        public Vector2 Data { get; set; }
    }
}