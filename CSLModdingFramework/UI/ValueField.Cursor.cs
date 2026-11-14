using ColossalFramework;
using UnityEngine;

namespace CSLModdingFramework.UI;

public partial class ValueField {
    public int AnchorIndex { get; private set; }
    public int CursorIndex { get; private set; }
    public int SelectionStart => Mathf.Min(AnchorIndex, CursorIndex);
    public int SelectionEnd => Mathf.Max(AnchorIndex, CursorIndex);
    public bool HasSelection => AnchorIndex != CursorIndex;

    public void MoveCursor(int index) => AnchorIndex = CursorIndex = index;

    public void UpdateSelection(int index) => CursorIndex = index;

    public void SetSelection(int start, int end) {
        AnchorIndex = start;
        CursorIndex = end;
    }

    public bool MoveCursorEnd() {
        if (string.IsNullOrEmpty(_text)) {
            return false;
        }

        MoveCursor(_text.Length);
        UpdateCursorLayout(CursorIndex);
        SetCursorLayoutDirty();
        return true;
    }

    public bool MoveCursorHome() {
        if (string.IsNullOrEmpty(_text)) {
            return false;
        }

        MoveCursor(0);
        UpdateCursorLayout(CursorIndex);
        SetCursorLayoutDirty();
        return true;
    }

    public bool MoveCursorLeft() {
        if (CursorIndex <= 0) return false;

        MoveCursor(CursorIndex - 1);
        UpdateCursorLayout(CursorIndex);
        SetCursorLayoutDirty();
        _cursorShown = true;
        Invalidate();
        return true;
    }

    public bool MoveCursorRight() {
        if (CursorIndex >= _text.Length) return false;

        MoveCursor(CursorIndex + 1);
        UpdateCursorLayout(CursorIndex);
        SetCursorLayoutDirty();
        _cursorShown = true;
        Invalidate();
        return true;
    }

    public bool MoveCursorUp() {
        var currentLine = GetHitLineIndex(new Vector2(_cursorX, _cursorY));
        if (currentLine <= 0) {
            return false;
        }

        var targetX = _cursorX;
        var targetY = _cursorY - (_lines[currentLine].Height + TextLineSpacing);

        GetCursorInfo(new Vector2(targetX, targetY), out _cursorX, out _cursorY, out var newIndex);

        CursorIndex = newIndex;
        AnchorIndex = newIndex;
        _cursorShown = true;
        Invalidate();
        return true;
    }

    public bool MoveCursorDown() {
        var currentLine = GetHitLineIndex(new Vector2(_cursorX, _cursorY));
        if (currentLine >= _lines.Count - 1) {
            return false;
        }

        var targetX = _cursorX;
        var targetY = _cursorY + (_lines[currentLine].Height + TextLineSpacing);

        GetCursorInfo(new Vector2(targetX, targetY), out _cursorX, out _cursorY, out var newIndex);

        CursorIndex = newIndex;
        AnchorIndex = newIndex;
        _cursorShown = true;
        Invalidate();
        return true;
    }

    private bool CutSelection() {
        if (!HasSelection || _readOnly) return false;

        var start = SelectionStart;
        var end = SelectionEnd;
        var selected = _text.Substring(start, end - start);
        Clipboard.text = selected;

        var result = DeleteSelection();
        if (!result) return false;

        Invalidate();
        return true;
    }

    private bool CopySelection() {
        if (!HasSelection) return false;

        var start = SelectionStart;
        var end = SelectionEnd;
        var selected = _text.Substring(start, end - start);
        Clipboard.text = selected;
        return true;
    }

    private bool SelectAll() {
        if (_text.Length == 0) {
            AnchorIndex = 0;
            CursorIndex = 0;
            return false;
        }

        SetSelection(0, _text.Length);
        Invalidate();
        return true;
    }

    private bool DeleteSelection(bool notify = true) {
        if (!HasSelection) return false;

        var start = SelectionStart;
        var length = SelectionEnd - SelectionStart;
        var deleted = _text.Substring(start, length);

        var op = new EditOperation {
            Kind = EditOperationKind.Delete,
            Index = start,
            Text = deleted,

            CursorBefore = CursorIndex,
            AnchorBefore = AnchorIndex,

            CursorAfter = start,
            AnchorAfter = start
        };

        PushUndo(op);

        _text = _text.Remove(start, length);
        if (notify)
            OnTextChanged();

        AnchorIndex = start;
        CursorIndex = start;
        return true;
    }

    public void ClearSelection() {
        if (!HasSelection) return;

        AnchorIndex = CursorIndex;
    }
}