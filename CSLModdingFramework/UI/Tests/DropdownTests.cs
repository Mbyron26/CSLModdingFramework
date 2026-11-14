using System.Collections.Generic;
using CSLModdingFramework.UI.Controls;
using UnityEngine;

namespace CSLModdingFramework.UI.Tests;

public class DropdownTests {
    private static Dropdown<string> _stringDropDownInstance;
    private static Dropdown<CustomDropDownValueClass> _customDropDownClass;
    public static List<string> TestList = new() { "ListOption 0", "ListOption 1", "ListOption 2", "ListOption 3" };
    public static string TestListDefaultValue = "ListOption 1";

    public static List<CustomDropDownValueClass> TestCustomList = new() {
        new CustomDropDownValueClass("TestCustomListItem 0", 0),
        new CustomDropDownValueClass("TestCustomListItem 1, long display text", 1),
        new CustomDropDownValueClass("TestCustomListItem 2", 2),
        new CustomDropDownValueClass("TestCustomListItem 3", 3)
    };

    public static CustomDropDownValueClass TestCustomListDefault { get; set; }

    public static Dropdown<CustomDropDownValueClass> CustomDropDownClass {
        get {
            if (_customDropDownClass is null) {
                _customDropDownClass = new Dropdown<CustomDropDownValueClass>() {
                    DefaultTextSelector  = v => v.DisplayText,
                };
                _customDropDownClass.AddOptions(TestCustomList);
                _customDropDownClass.ValueSelectionChanged += (i) => {
                    TestCustomListDefault = i;
                    Debug.Log($"Selected option: {i}");
                };
                TestCustomListDefault = TestCustomList[1];
                _customDropDownClass.SetSelectedWithoutNotify(v => ReferenceEquals(v.Value, TestCustomListDefault));
            }

            return _customDropDownClass;
        }
    }

    public static Dropdown<string> StringDropDownInstance {
        get {
            if (_stringDropDownInstance is null) {
                _stringDropDownInstance = new Dropdown<string>() { };
                DropDownTest2(_stringDropDownInstance);
            }

            return _stringDropDownInstance;
        }
    }

    public static void DropDownTest2(Dropdown<string> dropDown) {
        dropDown.AddOptions(TestList);
        dropDown.ValueSelectionChanged += (i) => {
            TestListDefaultValue = i;
            Debug.Log($"Selected option: {i}");
        };
        dropDown.SetSelectedWithoutNotify(v => string.Equals(v.Value, TestListDefaultValue));
    }

    public static void DropDownTest1(Dropdown<int> dropDown) {
        dropDown.AddOption(0, "Option 0");
        var t = dropDown.AddOption(1, "Option 1");
        dropDown.AddOption(2, "Option 2");
        dropDown.AddOption(3, "Option 3");
        dropDown.ValueSelectionChanged += (i) => Debug.Log($"Selected option: {i}");
        dropDown.SetSelectedWithoutNotify(t);
    }

    public class CustomDropDownValueClass {
        public string DisplayText { get; private set; }
        public ulong Data { get; private set; }

        public CustomDropDownValueClass(string displayText, ulong data) {
            DisplayText = displayText;
            Data = data;
        }

        public override string ToString() => $"{DisplayText} ({Data})";
    }
}