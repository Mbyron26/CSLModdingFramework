using UnityEngine;

namespace CSLModdingFramework.Core;

public static class ModifierFlagsExtensions {
    public static readonly string[] ModifierDisplayTable = {
        "", // 000
        "Shift +", // 001
        "Ctrl +", // 010
        "Ctrl + Shift +", // 011
        "Alt +", // 100
        "Alt + Shift +", // 101
        "Alt + Ctrl +", // 110
        "Alt + Ctrl + Shift +" // 111
    };

    public static bool HasFlag(this ModifierFlags flags, ModifierFlags flag) => (flags & flag) == flag;

    public static ModifierFlags SetFlag(this ModifierFlags flags, ModifierFlags flag) => flags | flag;

    public static ModifierFlags ClearFlag(this ModifierFlags flags, ModifierFlags flag) => flags & ~flag;

    public static ModifierFlags ToggleFlag(this ModifierFlags flags, ModifierFlags flag) => flags ^ flag;

    public static bool HasAny(this ModifierFlags flags) => flags != ModifierFlags.None;

    public static bool IsActive(this ModifierFlags flags) {
        if (flags.HasFlag(ModifierFlags.Control) && !IsControlDown()) return false;
        if (flags.HasFlag(ModifierFlags.Shift) && !IsShiftDown()) return false;
        if (flags.HasFlag(ModifierFlags.Alt) && !IsAltDown()) return false;
        if (flags.HasFlag(ModifierFlags.Meta) && !IsMetaDown()) return false;
        return true;
    }

    public static bool IsModifierKey(KeyCode code) => code is
        KeyCode.LeftControl or
        KeyCode.RightControl or
        KeyCode.LeftShift or
        KeyCode.RightShift or
        KeyCode.LeftAlt or
        KeyCode.RightAlt or
        KeyCode.AltGr;

    public static bool AnyModifierDown() => IsControlDown() || IsShiftDown() || IsAltDown();

    public static bool IsControlDown() => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

    public static bool IsShiftDown() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    public static bool IsAltDown() => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);

    public static bool IsMetaDown() => Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) || Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows);

    public static string GetActiveModifierDisplay(this ModifierFlags flags) {
        var mask = 0;
        if (flags.HasFlag(ModifierFlags.Shift)) mask |= 1;
        if (flags.HasFlag(ModifierFlags.Control)) mask |= 2;
        if (flags.HasFlag(ModifierFlags.Alt)) mask |= 4;
        return ModifierDisplayTable[mask];
    }

    public static string GetActiveModifierDisplay() {
        var mask = 0;
        if (IsShiftDown()) mask |= 1;
        if (IsControlDown()) mask |= 2;
        if (IsAltDown()) mask |= 4;
        return ModifierDisplayTable[mask];
    }
}