using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using CSLModdingFramework.Collections;
using CSLModdingFramework.UI.Fonts;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract partial class ValueField : TextControl {
    protected float _cursorBlinkElapsed;
    protected bool _cursorShown;
    protected float _cursorX;
    protected float _cursorY;
    protected bool _imeActive;
    protected string _lastCompositionString;
    protected float _lineHeight;
    protected List<TextLine> _lines;
    protected bool _readOnly;

    public event UIEventHandler<ValueField, bool> ReadOnlyChanged;
    public event UIEventHandler<ValueField, string> TextChanged;

    public override bool TextProcessMarkup => false;

    public float CursorBlinkInterval { get; set; } = 0.45f;
    public int CursorWidth { get; set; } = 1;
    public float ImeCursorYOffset { get; set; }

    public bool ReadOnly {
        get => _readOnly;
        set => SetReadOnly(value);
    }

    public bool SelectAllOnFocus { get; set; } = true;

    public virtual void SetReadOnly(bool value, bool notify = true) {
        if (value == _readOnly)
            return;

        _readOnly = value;

        if (notify)
            ReadOnlyChanged?.Invoke(this, value);

        Invalidate();
    }

    public virtual void SetReadOnlyWithoutNotify(bool value) => SetReadOnly(value, false);

    protected virtual void OnGotFocusInternal(UIFocusEventParameter p) {
        _imeActive = true;
        Input.imeCompositionMode = IMECompositionMode.On;
        if (SelectAllOnFocus && !string.IsNullOrEmpty(_text)) {
            SetSelection(0, _text.Length);
        }

        _cursorBlinkElapsed = 0f;
        if (_cursorShown) {
            _cursorShown = true;
        }

        Invalidate();
    }

    protected virtual void OnKeyDownInternal(UIKeyEventParameter p) {
        if (p.used) return;

        if (ExecuteEditCommand(p) || ExecuteCursorCommand(p) || ExecuteEditOperation(p)) {
            p.Use();
        }
    }

    protected virtual void OnKeyPressInternal(UIKeyEventParameter p) {
        if (p.used || _readOnly || char.IsControl(p.character) || !CanInsertNewChar()) return;

        DeleteSelection(false);
        InsertText(p.character.ToString());
        SetCursorLayoutDirty();
        PerformAutoLayout();
        UpdateIMEPosition();
        Invalidate();
        p.Use();
    }

    protected virtual void OnLostFocusInternal(UIFocusEventParameter p) {
        _imeActive = false;
        Input.imeCompositionMode = IMECompositionMode.Auto;
        _cursorBlinkElapsed = 0f;

        if (_cursorShown) {
            _cursorShown = false;
        }

        ClearSelection();
        Invalidate();
    }

    protected virtual void OnMouseDownInternal(UIMouseEventParameter p) {
        GetHitPosition(p.ray, out var hitPosition);
        GetCursorInfo(hitPosition, out var cursorX, out var cursorY, out var cursorIndex);
        _cursorX = cursorX;
        _cursorY = cursorY;
        UpdateIMEPosition();

        cursorIndex = Mathf.Clamp(cursorIndex, 0, _text.Length);
        if (cursorIndex != CursorIndex || HasSelection) {
            MoveCursor(cursorIndex);
            _cursorShown = true;
            Invalidate();
            p.Use();
        }
    }

    protected virtual void OnMouseMoveInternal(UIMouseEventParameter p) {
        if (!hasFocus || !p.buttons.IsFlagSet(UIMouseButton.Left)) return;

        GetCursorInfo(GetHitPosition(p), out _, out _, out var cursorIndex);
        if (cursorIndex != CursorIndex) {
            UpdateSelection(cursorIndex);
            Invalidate();
            p.Use();
        }
    }

    public override void Awake() {
        base.Awake();

        m_CanFocus = true;
        _lines = ListPool<TextLine>.Rent(8);
        BlurOnClickOutside = true;
        TextColors.SetValues(UIColorPalette.ClassicDarkPalette.HeaderTextColors);
    }

    public override void OnDestroy() {
        base.OnDestroy();

        foreach (var textLine in _lines) {
            textLine.Return();
        }

        _lines.Clear();
        ListPool<TextLine>.Return(_lines);
    }

    public override bool SetText(string text, bool notify = true) {
        if (string.Equals(text, _text)) return false;

        _text = text;
        SetCursorLayoutDirty();
        InvalidateAutoLayout();

        if (notify)
            OnTextChanged();

        return true;
    }

    public override void Update() {
        base.Update();

        UpdateCursorBlink();
        if (UpdateComposition())
            Invalidate();
    }

    protected override void OnGotFocus(UIFocusEventParameter p) {
        base.OnGotFocus(p);

        OnGotFocusInternal(p);
    }

    protected override void OnKeyDown(UIKeyEventParameter p) {
        base.OnKeyDown(p);

        OnKeyDownInternal(p);
    }

    protected override void OnKeyPress(UIKeyEventParameter p) {
        base.OnKeyPress(p);

        OnKeyPressInternal(p);
    }

    protected override void OnLostFocus(UIFocusEventParameter p) {
        base.OnLostFocus(p);

        OnLostFocusInternal(p);
    }

    protected override void OnMouseDown(UIMouseEventParameter p) {
        OnMouseDownInternal(p);

        base.OnMouseDown(p);
    }

    protected override void OnMouseMove(UIMouseEventParameter p) {
        OnMouseMoveInternal(p);

        base.OnMouseMove(p);
    }

    protected override void OnSizeChanged() {
        PerformAutoLayout();

        base.OnSizeChanged();
    }

    protected override void OnTextChanged() => TextChanged?.Invoke(this, _text);

    protected int CalculateMaxLines() {
        var availableHeight = height - _textPadding.Vertical;
        var lineAdvance = _lineHeight + TextLineSpacing;
        return lineAdvance <= 0 ? 4096 : Mathf.FloorToInt((availableHeight + TextLineSpacing) / lineAdvance);
    }

    protected bool CanInsertNewChar() => !_textWordWrap || _lines.Count <= CalculateMaxLines();
    protected bool CanInsertNewLine() => _lines.Count + 1 <= CalculateMaxLines();

    protected void GetCursorInfo(Vector2 hitPosition, out float cursorX, out float cursorY, out int cursorIndex) {
        if (_textWordWrap) {
            var lineIndex = GetHitLineIndex(hitPosition);
            if (lineIndex < 0 || lineIndex >= _lines.Count) {
                cursorX = 0f;
                cursorY = 0f;
                cursorIndex = 0;
                return;
            }

            var line = _lines[lineIndex];

            GetLineCursorInfo(
                hitPosition.x,
                _textRenderPosition.x,
                line.CharOffsets,
                line.CharCount + 1,
                out var lineCursorIndex,
                out cursorX);

            cursorIndex = line.StartIndex + lineCursorIndex;

            cursorY = _textRenderPosition.y
                      + lineIndex * (line.Height + TextLineSpacing);

            Debug.Log(
                $"lineIndex={lineIndex}, " +
                $"lineCursorIndex={lineCursorIndex}, " +
                $"line.StartIndex={line.StartIndex}, " +
                $"cursorIndex={cursorIndex}, " +
                $"cursorX={cursorX}, " +
                $"cursorY={cursorY}");
        }
        else {
            if (string.IsNullOrEmpty(_text)) {
                cursorX = 0f;
                cursorY = 0f;
                cursorIndex = 0;

                return;
            }

            if (_lines.Count == 0) {
                cursorX = _textRenderPosition.x;
                cursorY = _textRenderPosition.y;
                cursorIndex = 0;

                return;
            }

            var line = _lines.First();

            GetLineCursorInfo(
                hitPosition.x,
                _textRenderPosition.x,
                line.CharOffsets,
                line.CharCount + 1,
                out cursorIndex,
                out cursorX);

            cursorY = _textRenderPosition.y;
        }
    }

    protected void GetLineCursorInfo(
        float hitX,
        float lineRenderStartX,
        float[] charOffsets,
        int charOffsetsCount,
        out int lineCursorIndex,
        out float cursorRenderPositionX) {
        var localX = hitX - lineRenderStartX;

        if (localX <= 0f) {
            cursorRenderPositionX = lineRenderStartX + charOffsets[0];
            lineCursorIndex = 0;
            return;
        }

        for (var i = 0; i < charOffsetsCount - 1; i++) {
            var start = charOffsets[i];
            var end = charOffsets[i + 1];
            var mid = (start + end) * 0.5f;

            if (localX < mid) {
                cursorRenderPositionX = lineRenderStartX + start;
                lineCursorIndex = i;
                return;
            }

            if (localX < end) {
                cursorRenderPositionX = lineRenderStartX + end;
                lineCursorIndex = i + 1;
                return;
            }
        }

        lineCursorIndex = charOffsetsCount - 1;
        cursorRenderPositionX = lineRenderStartX + charOffsets[lineCursorIndex];
    }

    protected int HitCursorIndex(float hitX, float renderOffsetX, float[] charOffsets, int charCount, out float cursorX) {
        var localX = hitX - renderOffsetX;

        Debug.Log(
            $"hitX={hitX}, renderX={renderOffsetX}, localX={localX}");

        if (localX <= 0f) {
            cursorX = renderOffsetX + charOffsets[0];
            return 0;
        }

        for (var i = 0; i < charCount; i++) {
            var start = charOffsets[i];
            var end = charOffsets[i + 1];

            var mid = (start + end) * 0.5f;

            if (localX < mid) {
                cursorX = renderOffsetX + start;
                return i;
            }

            if (localX < end) {
                cursorX = renderOffsetX + end;
                return i + 1;
            }
        }

        cursorX = renderOffsetX + charOffsets[charCount];
        return charCount;
    }

    private int GetHitLineIndex(Vector2 hitPosition) {
        var y = hitPosition.y - _textRenderPosition.y;

        var currentY = 0f;
        var lineSpacing = TextLineSpacing;

        for (var i = 0; i < _lines.Count; i++) {
            var lineHeight = _lines[i].Height;

            if (y < currentY + lineHeight) {
                return i;
            }

            currentY += lineHeight + lineSpacing;
        }

        return _lines.Count - 1;
    }

    private void ReleaseTextLineListItems() {
        foreach (var textLine in _lines) {
            textLine.Return();
        }

        _lines.Clear();
    }

    private bool UpdateComposition() {
        if (!_imeActive || _readOnly) return false;

        var composition = Input.compositionString;

        if (_lastCompositionString == composition)
            return false;

        _lastCompositionString = composition;
        return true;
    }

    private void UpdateCursorBlink() {
        if (!hasFocus) return;

        _cursorBlinkElapsed += Time.deltaTime;

        if (_cursorBlinkElapsed < CursorBlinkInterval)
            return;

        _cursorBlinkElapsed = 0f;

        _cursorShown = !_cursorShown;

        Invalidate();
    }

    private void UpdateIMEPosition() {
        var uiView = GetUIView();
        var pixelWidthScale = uiView.uiCamera.pixelWidth / (float)uiView.fixedWidth;
        var pixelHeightScale = uiView.uiCamera.pixelHeight / (float)uiView.fixedHeight;
        var vector3 = uiView.uiCamera.WorldToScreenPoint(transform.position);
        vector3.y = Screen.height - vector3.y;
        Input.compositionCursorPos = new Vector2(vector3.x + _cursorX * pixelWidthScale, vector3.y + _cursorY * pixelHeightScale + ImeCursorYOffset);
    }
}