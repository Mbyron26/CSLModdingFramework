using UnityEngine;

namespace CSLModdingFramework.UI;

public class UIColorPalette {
    public static Color32 Black { get; } = new(0, 0, 0, 255);
    public static Color32 White { get; } = new(255, 255, 255, 255);
    public static Color32 White90Percent { get; } = new(230, 230, 230, 255);
    public static Color32 White80Percent { get; } = new(204, 204, 204, 255);
    public static Color32 White70Percent { get; } = new(179, 179, 179, 255);
    public static Color32 White60Percent { get; } = new(153, 153, 153, 255);
    public static Color32 White50Percent { get; } = new(128, 128, 128, 255);
    public static Color32 White40Percent { get; } = new(102, 102, 102, 255);
    public static Color32 White30Percent { get; } = new(77, 77, 77, 255);
    public static Color32 White20Percent { get; } = new(51, 51, 51, 255);
    public static Color32 White10Percent { get; } = new(25, 25, 25, 255);
    public static ColorStateSet HeaderTextColors { get; } = new(White, White, White, White, White60Percent);
    public static ColorStateSet DescriptionTextColors { get; } = new(White60Percent, White60Percent, White60Percent, White60Percent, White40Percent);

    public static Color32 BlueNormal { get; } = new(10, 92, 186, 255);
    public static Color32 BlueHovered { get; } = new(40, 126, 226, 255);
    public static Color32 BluePressed { get; } = new(18, 102, 200, 255);
    public static Color32 BlueFocused { get; } = BlueNormal;
    public static Color32 BlueDisabled { get; } = new(22, 42, 76, 255);
    public static ColorStateSet BlueColors { get; } = new(BlueNormal, BlueHovered, BluePressed, BlueFocused, BlueDisabled);

    public static Color32 GreenNormal { get; } = new(76, 164, 50, 255);
    public static Color32 GreenHovered { get; } = new(86, 184, 58, 255);
    public static Color32 GreenPressed { get; } = new(72, 154, 46, 255);
    public static Color32 GreenFocused => GreenNormal;
    public static Color32 GreenDisabled { get; } = new(44, 86, 44, 255);
    public static ColorStateSet GreenColors { get; } = new(GreenNormal, GreenHovered, GreenPressed, GreenFocused, GreenDisabled);

    public static Color32 YellowNormal => new(188, 120, 6, 255);
    public static Color32 YellowHovered => new(198, 140, 16, 255);
    public static Color32 YellowPressed => new(178, 110, 0, 255);
    public static Color32 YellowFocused => GreenNormal;
    public static Color32 YellowDisabled => new(128, 100, 0, 255);
    public static ColorStateSet YellowColors { get; } = new(YellowNormal, YellowHovered, YellowPressed, YellowFocused, YellowDisabled);

    public static Color32 RedNormal { get; } = new Color32(150, 24, 26, 255);
    public static Color32 RedHovered { get; } = new Color32(180, 50, 52, 255);
    public static Color32 RedPressed { get; } = new Color32(130, 20, 22, 255);
    public static Color32 RedFocused { get; } = RedNormal;
    public static Color32 RedDisabled { get; } = new Color32(90, 14, 16, 255);
    public static ColorStateSet RedColors { get; } = new(RedNormal, RedHovered, RedPressed, RedFocused, RedDisabled);

    public static Color32 ToggleForegroundNormal { get; } = new(220, 220, 220, 255);
    public static Color32 ToggleForegroundDisabled { get; } = new(110, 110, 110, 255);

    public class ModernDarkPalette {
        public static Color32 RootBackground { get; } = new(20, 24, 38, 255);
        public static Color32 SettingsSectionBackground { get; } = new(30, 40, 50, 255);
        public static Color32 SettingsSectionForeground { get; } = new(85, 100, 111, 255);
        public static Color32 SettingItemBackground { get; } = new(45, 53, 65, 255);

        public static Color32 ElementNormal { get; } = new(50, 60, 70, 255);
        public static Color32 ElementHovered { get; } = new(62, 73, 86, 255);
        public static Color32 ElementPressed { get; } = new(77, 91, 108, 255);
        public static Color32 ElementFocused => GreenNormal;
        public static Color32 ElementDisabled { get; } = new(46, 55, 64, 255);

        public static ColorStateSet ElementColors { get; } = new(ElementNormal, ElementHovered, ElementPressed, ElementFocused, ElementDisabled);
    }

    public class ClassicLightPalette {
        public static Color32 SettingsSectionBackground { get; } = new(70, 94, 114, 255);
        public static Color32 SettingsSectionBackgroundHovered { get; } = new(76, 102, 122, 255);
        public static Color32 SettingItemBackground => new(146, 162, 174, 255);

        public static Color32 ElementNormal { get; } = new(92, 120, 140, 255);
        public static Color32 ElementHovered { get; } = new(104, 134, 156, 255);
        public static Color32 ElementPressed { get; } = new(86, 114, 132, 255);
        public static Color32 ElementFocused => GreenNormal;
        public static Color32 ElementDisabled { get; } = new(52, 70, 86, 255);

        public static ColorStateSet SettingItemControlColors { get; } = new(ElementNormal, ElementHovered, ElementPressed, ElementFocused, ElementDisabled);
    }

    public class ClassicDarkPalette {
        public static Color32 RootBackground { get; } = new Color32(130, 130, 130, 255);
        public static Color32 RootForeground { get; } = new Color32(81, 94, 101, 255);

        public static Color32 HeaderTextDisabledColor => White40Percent;
        public static ColorStateSet HeaderTextColors { get; } = new(White, White, White, White, White40Percent);
        public static Color32 DescriptionTextColor => White50Percent;
        public static Color32 DescriptionTextDisabledColor => White20Percent;

        public static Color32 SettingsSectionBackground { get; } = new(36, 48, 62, 255);
        public static Color32 SettingsSectionBackgroundHovered { get; } = new(43, 56, 71, 255);
        public static Color32 SettingsSectionBackgroundPressed { get; } = new(36, 48, 62, 255);
        public static Color32 SettingsSectionBackgroundFocused => BlueNormal;
        public static Color32 SettingsSectionBackgroundDisabled { get; } = new(32, 42, 53, 255);
        public static Color32 SettingItemBackground { get; } = new(78, 86, 100, 255);

        public static Color32 SettingItemControlBackground { get; } = new(56, 70, 86, 255);
        public static Color32 SettingItemControlBackgroundHovered { get; } = new(70, 84, 102, 255);
        public static Color32 SettingItemControlBackgroundPressed { get; } = new(80, 94, 112, 255);
        public static Color32 SettingItemControlBackgroundFocused => BlueNormal;
        public static Color32 SettingItemControlBackgroundDisabled { get; } = new(46, 58, 70, 255);

        public static ColorStateSet SettingsSectionColors { get; } = new(SettingsSectionBackground, SettingsSectionBackgroundHovered, SettingsSectionBackgroundPressed, SettingsSectionBackgroundFocused, SettingsSectionBackgroundDisabled);
        public static ColorStateSet SettingItemControlColors { get; } = new(SettingItemControlBackground, SettingItemControlBackgroundHovered, SettingItemControlBackgroundPressed, SettingItemControlBackgroundFocused, SettingItemControlBackgroundDisabled);
    }
}