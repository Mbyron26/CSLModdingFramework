using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI;

public partial class ValueField {
    private bool ExecuteCursorCommand(UIKeyEventParameter p) => p.keycode switch {
        KeyCode.UpArrow => MoveCursorUp(),
        KeyCode.DownArrow => MoveCursorDown(),
        KeyCode.LeftArrow => MoveCursorLeft(),
        KeyCode.RightArrow => MoveCursorRight(),
        KeyCode.Home => MoveCursorHome(),
        KeyCode.End => MoveCursorEnd(),
        _ => false
    };

    private bool ExecuteEditOperation(UIKeyEventParameter p) => p.keycode switch {
        KeyCode.Backspace => ProcessBackspaceKey(p),
        KeyCode.Delete => ProcessDeleteKey(p),
        KeyCode.Return => InsertNewLine(),
        _ => false
    };

    private bool ExecuteEditCommand(UIKeyEventParameter p) {
        if (!p.control) return false;

        return p.keycode switch {
            KeyCode.A => SelectAll(),
            KeyCode.C => CopySelection(),
            KeyCode.X => CutSelection(),
            KeyCode.V => Paste(),
            KeyCode.Z => Undo(),
            KeyCode.Y => Redo(),
            _ => false
        };
    }

    private bool ProcessDeleteKey(UIKeyEventParameter p) {
        if (_readOnly) return false;

        if (HasSelection) {
            DeleteSelection();
        }
        else if (p.control) {
            DeleteNextWord();
        }
        else {
            DeleteNextCharacter();
        }

        SetCursorLayoutDirty();
        InvalidateAutoLayout();
        return true;
    }

    private bool ProcessBackspaceKey(UIKeyEventParameter p) {
        if (_readOnly) return false;
        if (HasSelection) {
            DeleteSelection();
        }
        else if (p.control) {
            DeletePreviousWord();
        }
        else {
            DeletePreviousCharacter();
        }

        SetCursorLayoutDirty();
        InvalidateAutoLayout();
        return true;
    }
}