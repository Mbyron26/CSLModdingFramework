using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class ColorHexFieldBase<T> : ValueField {
    public const string HexPrefix = "#";
    protected ColorHexFormat _hexFormat = ColorHexFormat.RGB;

    public virtual ColorHexFormat HexFormat {
        get => _hexFormat;
        set => SetHexFormat(value);
    }

    public override TextSizeMode SizeMode => _sizeMode;

    public abstract bool SetValue(T value, bool notify = true);
    public abstract bool SetValueWithoutNotify(T value);
    protected abstract string FormatValue(T value);
    protected abstract void SetHexFormat(ColorHexFormat value);
    protected abstract bool TryParseValue(string text, out T value);
    protected abstract void UpdateDisplayText();

    protected override void OnLostFocusInternal(UIFocusEventParameter p) {
        _imeActive = false;
        Input.imeCompositionMode = IMECompositionMode.Auto;
        _cursorBlinkElapsed = 0f;

        if (_cursorShown) {
            _cursorShown = false;
        }

        ClearSelection();

        if (TryParseValue(_text, out var value)) {
            if (!SetValue(value)) {
                UpdateDisplayText();
            }
        }
        else {
            UpdateDisplayText();
        }

        Invalidate();
    }
}