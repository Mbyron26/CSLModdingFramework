using System.Collections.Generic;
using CSLModdingFramework.UI.Controls;

namespace CSLModdingFramework.UI.Tests;

public class CheckBoxTests {
    public static ClassCheckBoxItem ClassCheckBoxItem1 = new() {
        Header = "ClassCheckBoxItem1",
        DataInt = 1,
        DataBool = true,
        Description = "This is item 1"
    };

    public static ClassCheckBoxItem ClassCheckBoxItem2 = new() {
        Header = "ClassCheckBoxItem2",
        DataInt = 2,
        DataBool = false,
        Description = "This is item 2"
    };

    public static ClassCheckBoxItem ClassCheckBoxItem3 = new() {
        Header = "ClassCheckBoxItem3",
        DataInt = 3,
        DataBool = true,
        Description = "This is item 3"
    };

    public static List<ClassCheckBoxItem> ClassCheckBoxItems = new() {
        ClassCheckBoxItem1, ClassCheckBoxItem2, ClassCheckBoxItem3
    };

    private static CheckboxGroup<ClassCheckBoxItem> _classCheckBoxGroup;
    private static ScrollableCheckboxGroup<ClassCheckBoxItem> _scrollableClassCheckBoxGroup;

    public static CheckboxGroup<ClassCheckBoxItem> ClassCheckBoxGroup {
        get {
            if (_classCheckBoxGroup == null) {
                _classCheckBoxGroup = new CheckboxGroup<ClassCheckBoxItem>();
                ClassCheckBoxItemTest2(ClassCheckBoxGroup);
            }

            return _classCheckBoxGroup;
        }
    }

    public static ScrollableCheckboxGroup<ClassCheckBoxItem> ScrollableClassCheckBoxGroup {
        get {
            if (_scrollableClassCheckBoxGroup == null) {
                _scrollableClassCheckBoxGroup = new ScrollableCheckboxGroup<ClassCheckBoxItem>();
                ClassCheckBoxItemTest2(_scrollableClassCheckBoxGroup);
            }

            return _scrollableClassCheckBoxGroup;
        }
    }

    public static bool BoolCheckBoxItem1 { get; set; }
    public static bool BoolCheckBoxItem2 { get; set; }
    public static bool BoolCheckBoxItem3 { get; set; } = true;

    private static CheckboxGroup<bool> _boolCheckBoxGroup;

    public static CheckboxGroup<bool> BoolCheckBoxGroup {
        get {
            if (_boolCheckBoxGroup == null) {
                _boolCheckBoxGroup = new CheckboxGroup<bool>();
                BoolCheckBoxItemTest1(_boolCheckBoxGroup);
            }

            return _boolCheckBoxGroup;
        }
    }

    public static void BoolCheckBoxItemTest1(CheckboxGroup<bool> checkBoxGroup) {
        _boolCheckBoxGroup.CheckboxCreated += box => { box.clipChildren = true; };
        _boolCheckBoxGroup.IsIconOnlyClickable = true;
        checkBoxGroup.AddOption(BoolCheckBoxItem1, BoolCheckBoxItem1, nameof(BoolCheckBoxItem1), onCheckedChanged: b => BoolCheckBoxItem1 = b.Value);

        checkBoxGroup.AddOption(BoolCheckBoxItem2, BoolCheckBoxItem2, nameof(BoolCheckBoxItem2), onCheckedChanged: b => BoolCheckBoxItem2 = b.Value);

        checkBoxGroup.AddOption(BoolCheckBoxItem3, BoolCheckBoxItem3, nameof(BoolCheckBoxItem3), onCheckedChanged: b => BoolCheckBoxItem3 = b.Value);
    }

    public static void ClassCheckBoxItemTest1(CheckboxGroup<ClassCheckBoxItem> checkBoxGroup) {
        checkBoxGroup.AddSelectAllCheckBox(c => {
            c.Header = "Check All/ Uncheck All";
            c.SetTheme(ThemeKind.ClassicDark);
        });
        checkBoxGroup.AddOption(ClassCheckBoxItem1, ClassCheckBoxItem1.DataBool, ClassCheckBoxItem1.Header, onCheckedChanged: b => ClassCheckBoxItem1.DataBool = b.Value.DataBool);
        checkBoxGroup.AddOption(ClassCheckBoxItem2, ClassCheckBoxItem2.DataBool, ClassCheckBoxItem2.Header, onCheckedChanged: b => ClassCheckBoxItem2.DataBool = b.Value.DataBool);
        checkBoxGroup.AddOption(ClassCheckBoxItem3, ClassCheckBoxItem3.DataBool, ClassCheckBoxItem3.Header, onCheckedChanged: b => ClassCheckBoxItem3.DataBool = b.Value.DataBool);
    }

    public static void ClassCheckBoxItemTest2(CheckboxGroupBase<ClassCheckBoxItem> checkBoxGroup) {
        checkBoxGroup.SelectAllCheckboxCreated += c => c.SetTheme(ThemeKind.ClassicDark);
        checkBoxGroup.CheckboxCreated += c => c.SetTheme(ThemeKind.ClassicDark);
        checkBoxGroup.DefaultHeaderSelector = item => item.Header;
        checkBoxGroup.DefaultDescriptionSelector = item => item.Description;
        checkBoxGroup.DefaultCheckedSelector = item => item.DataBool;
        checkBoxGroup.DefaultCheckedCallbackSelector = b => (boxItem) => boxItem.Value.DataBool = b.DataBool;
        checkBoxGroup.AddOptions(ClassCheckBoxItems);
    }

    public class ClassCheckBoxItem {
        public string Header;
        public int DataInt;
        public bool DataBool;
        public string Description;
    }
}