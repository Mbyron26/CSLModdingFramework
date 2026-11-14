using CSLModdingFramework.UI.Controls;
using UnityEngine;

namespace CSLModdingFramework.UI.Tests;

public static class RadioGroupTests {
    private static RadioGroup<RadioGroupClassTest> _radioGroup;

    public static RadioGroupClassTest RadioGroupClassTestValue0 { get; set; } = new() {
        Header = "Header 0",
        Description = "Description 0",
        Data = new Vector2(0, 0)
    };

    public static RadioGroupClassTest RadioGroupClassTestValue1 { get; set; } = new() {
        Header = "Header 1",
        Description = "Description 1",
        Data = new Vector2(1, 1)
    };

    public static RadioGroupClassTest RadioGroupClassTestValue2 { get; set; } = new() {
        Header = "Header 2",
        Description = "Description 2",
        Data = new Vector2(2, 2)
    };

    public static RadioGroupClassTest RadioGroupClassTestValue3 { get; set; } = new() {
        Header = "Header 3",
        Description = "Description 3",
        Data = new Vector2(3, 3)
    };

    public static Vector2 SelectedData { get; set; } = new(3, 3);

    public static RadioGroup<RadioGroupClassTest> RadioGroup {
        get {
            if (_radioGroup is null) {
                _radioGroup = new RadioGroup<RadioGroupClassTest>();
                _radioGroup.RadioButtonCreated += radioButton => radioButton.SetTheme(ThemeKind.ClassicDark);
                _radioGroup.DefaultHeaderSelector = value => value.Header;
                _radioGroup.DefaultDescriptionSelector = value => value.Description;
                _radioGroup.AddOption(RadioGroupClassTestValue0);
                _radioGroup.AddOption(RadioGroupClassTestValue1);
                _radioGroup.AddOption(RadioGroupClassTestValue2);
                _radioGroup.AddOption(RadioGroupClassTestValue3);
                _radioGroup.SetSelectedWithoutNotify(value => value.Value.Data == SelectedData);
                _radioGroup.ValueSelectionChanged += option => {
                    SelectedData = option.Data;
                    Debug.Log($"Selected: {option.Data}");
                };
            }

            return _radioGroup;
        }
    }

    public class RadioGroupClassTest {
        public string Header { get; set; }
        public string Description { get; set; }
        public Vector2 Data { get; set; }
    }
}