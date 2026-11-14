using System.Collections.Generic;
using System.Text;
using ColossalFramework;
using CSLModdingFramework.UI.Utilities;
using UnityEngine;

namespace CSLModdingFramework.UI;

public partial class ValueField {
    private int _undoLimit = 20;
    private readonly List<EditOperation> _history = new();
    private int _currentIndex = -1;

    public int UndoLimit {
        get => _undoLimit;
        set {
            var newValue = Mathf.Max(0, value);
            if (newValue == _undoLimit) return;

            _undoLimit = value;
            TrimHistoryIfNeeded();
        }
    }

    protected virtual bool InsertText(string text, bool notify = true) {
        if (string.IsNullOrEmpty(text)) return false;

        var index = CursorIndex;

        var op = new EditOperation {
            Kind = EditOperationKind.Insert,
            Index = index,
            Text = text,

            CursorBefore = CursorIndex,
            AnchorBefore = AnchorIndex
        };


        if (index == _text.Length)
            _text += text;
        else
            _text = _text.Insert(index, text);

        CursorIndex += text.Length;
        AnchorIndex = CursorIndex;

        op.CursorAfter = CursorIndex;
        op.AnchorAfter = AnchorIndex;
        PushUndo(op);

        if (notify)
            OnTextChanged();

        return true;
    }

    public void DeleteText(int index, int length) {
        if (length <= 0 || index < 0 || index >= _text.Length)
            return;

        var safeLength = Mathf.Min(length, _text.Length - index);
        Text = _text.Remove(index, safeLength);
    }

    private bool Paste() {
        if (_readOnly) return false;
        var clipboardText = Clipboard.text;
        if (string.IsNullOrEmpty(clipboardText)) return false;

        var index = CursorIndex;
        string deleted = null;

        if (HasSelection) {
            var start = SelectionStart;
            var length = SelectionEnd - SelectionStart;
            deleted = _text.Substring(start, length);

            _text = _text.Remove(start, length);
            index = start;
        }

        var op = new EditOperation {
            Kind = EditOperationKind.Insert,
            Index = index,
            Text = (deleted ?? string.Empty) + clipboardText,

            CursorBefore = CursorIndex,
            AnchorBefore = AnchorIndex,

            CursorAfter = index + clipboardText.Length,
            AnchorAfter = index + clipboardText.Length
        };

        PushUndo(op);

        var sbInsert = new StringBuilder(_text);
        sbInsert.Insert(index, clipboardText);
        _text = sbInsert.ToString();
        OnTextChanged();

        CursorIndex = index + clipboardText.Length;
        AnchorIndex = CursorIndex;
        SetCursorLayoutDirty();
        InvalidateAutoLayout();
        return true;
    }

    public bool InsertNewLine() {
        if (_readOnly) return false;
        
        if (!_textWordWrap) {
            Unfocus();
            return false;
        }

        if (!CanInsertNewLine()) return false;

        var index = CursorIndex;

        string deleted = null;
        if (HasSelection) {
            var start = SelectionStart;
            var length = SelectionEnd - SelectionStart;
            deleted = _text.Substring(start, length);

            _text = _text.Remove(start, length);
            index = start;
        }

        const string newline = "\n";

        var op = new EditOperation {
            Kind = EditOperationKind.Insert,
            Index = index,
            Text = deleted != null ? deleted + newline : newline,

            CursorBefore = CursorIndex,
            AnchorBefore = AnchorIndex,

            CursorAfter = index + newline.Length,
            AnchorAfter = index + newline.Length
        };

        PushUndo(op);

        _text = _text.Insert(index, newline);
        OnTextChanged();

        CursorIndex = index + newline.Length;
        AnchorIndex = CursorIndex;
        SetCursorLayoutDirty();
        InvalidateAutoLayout();
        return true;
    }


    public void DeleteNextWord() {
        var i = CursorIndex;
        if (i >= _text.Length) return;

        var right = i;

        if (char.IsHighSurrogate(_text[right])) {
            var length = 2;
            var deleted = _text.Substring(right, length);

            var op = new EditOperation {
                Kind = EditOperationKind.Delete,
                Index = right,
                Text = deleted,
                CursorBefore = CursorIndex,
                AnchorBefore = AnchorIndex,
                CursorAfter = right,
                AnchorAfter = right
            };

            PushUndo(op);

            _text = _text.Remove(right, length);
            OnTextChanged();

            CursorIndex = right;
            AnchorIndex = right;
            return;
        }

        if (TextUtilities.IsCjk(_text[right])) {
            var length = 1;
            var deleted = _text.Substring(right, length);

            var op = new EditOperation {
                Kind = EditOperationKind.Delete,
                Index = right,
                Text = deleted,
                CursorBefore = CursorIndex,
                AnchorBefore = AnchorIndex,
                CursorAfter = right,
                AnchorAfter = right
            };

            PushUndo(op);

            _text = _text.Remove(right, length);
            OnTextChanged();

            CursorIndex = right;
            AnchorIndex = right;
            return;
        }

        while (right < _text.Length && char.IsWhiteSpace(_text[right]))
            right++;

        while (right < _text.Length && !char.IsWhiteSpace(_text[right]) && !TextUtilities.IsCjk(_text[right]))
            right++;

        var length2 = right - i;

        if (length2 <= 0)
            return;

        var deleted2 = _text.Substring(i, length2);

        var op2 = new EditOperation {
            Kind = EditOperationKind.Delete,
            Index = i,
            Text = deleted2,
            CursorBefore = CursorIndex,
            AnchorBefore = AnchorIndex,
            CursorAfter = i,
            AnchorAfter = i
        };

        PushUndo(op2);

        _text = _text.Remove(i, length2);
        OnTextChanged();

        CursorIndex = i;
        AnchorIndex = i;
    }

    private void DeleteNextCharacter() {
        if (CursorIndex >= _text.Length) return;

        var start = CursorIndex;
        var length = 1;

        if (char.IsHighSurrogate(_text[start]) && start + 1 < _text.Length) {
            length = 2;
        }

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
        OnTextChanged();

        CursorIndex = start;
        AnchorIndex = start;
    }

    private void DeletePreviousCharacter() {
        if (CursorIndex <= 0) return;

        var start = CursorIndex - 1;
        var length = 1;

        if (char.IsLowSurrogate(_text[start])) {
            start -= 1;
            length = 2;
        }

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
        OnTextChanged();

        CursorIndex = start;
        AnchorIndex = start;
    }

    private void DeletePreviousWord() {
        var i = CursorIndex;
        if (i <= 0) return;

        var left = i - 1;

        if (TextUtilities.IsCjk(_text[left])) {
            const int length = 1;

            var deleted = _text.Substring(left, length);

            var op = new EditOperation {
                Kind = EditOperationKind.Delete,
                Index = left,
                Text = deleted,
                CursorBefore = CursorIndex,
                AnchorBefore = AnchorIndex,
                CursorAfter = left,
                AnchorAfter = left
            };

            PushUndo(op);

            _text = _text.Remove(left, length);
            OnTextChanged();

            CursorIndex = left;
            AnchorIndex = left;
            return;
        }

        while (left >= 0 && char.IsWhiteSpace(_text[left]))
            left--;

        while (left >= 0 && !char.IsWhiteSpace(_text[left]) && !TextUtilities.IsCjk(_text[left]))
            left--;

        var start2 = left + 1;
        var length2 = i - start2;

        if (length2 <= 0)
            return;

        var deleted2 = _text.Substring(start2, length2);

        var op2 = new EditOperation {
            Kind = EditOperationKind.Delete,
            Index = start2,
            Text = deleted2,
            CursorBefore = CursorIndex,
            AnchorBefore = AnchorIndex,
            CursorAfter = start2,
            AnchorAfter = start2
        };

        PushUndo(op2);

        _text = _text.Remove(start2, length2);
        OnTextChanged();

        CursorIndex = start2;
        AnchorIndex = start2;
    }

    public bool Redo() {
        if (_readOnly || _currentIndex >= _history.Count - 1) return false;

        _currentIndex++;
        var op = _history[_currentIndex];
        ApplyOp(op);
        return true;
    }

    public bool Undo() {
        if (_readOnly || _currentIndex < 0) return false;

        var op = _history[_currentIndex];
        ApplyInverseOp(op);
        _currentIndex--;
        return true;
    }

    private void ApplyInverseOp(EditOperation op) {
        _text = op.Kind == EditOperationKind.Insert ? _text.Remove(op.Index, op.Text.Length) : _text.Insert(op.Index, op.Text);

        CursorIndex = op.CursorBefore;
        AnchorIndex = op.AnchorBefore;

        OnTextChanged();
    }

    private void ApplyOp(EditOperation op) {
        if (op.Kind == EditOperationKind.Insert) {
            _text = _text.Insert(op.Index, op.Text);
        }
        else {
            _text = _text.Remove(op.Index, op.Text.Length);
        }

        CursorIndex = op.CursorAfter;
        AnchorIndex = op.AnchorAfter;

        OnTextChanged();
    }

    private void PushUndo(EditOperation op) {
        if (_currentIndex < _history.Count - 1) {
            _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);
        }

        _history.Add(op);
        _currentIndex = _history.Count - 1;

        if (_history.Count > UndoLimit) {
            _history.RemoveAt(0);
            _currentIndex--;
        }
    }

    private void TrimHistoryIfNeeded() {
        if (_undoLimit == 0) {
            _history.Clear();
            _currentIndex = -1;
            return;
        }

        if (_history.Count > _undoLimit) {
            var removeCount = _history.Count - _undoLimit;
            _history.RemoveRange(0, removeCount);
            _currentIndex -= removeCount;
            if (_currentIndex < -1) _currentIndex = -1;
        }
    }
}