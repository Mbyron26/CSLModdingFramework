using System.Globalization;
using ColossalFramework.UI;
using CSLModdingFramework.Extensions;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class Color32HexField : ColorHexFieldBase<Color32> {
    protected Color32 _value = new(255, 255, 255, 255);

    public event UIEventHandler<Color32HexField, Color32> ValueChanged;

    public virtual Color32 Value {
        get => _value;
        set => SetValue(value);
    }

    public override bool SetValueWithoutNotify(Color32 value) => SetValue(value, false);

    public override bool SetValue(Color32 value, bool notify = true) {
        value = NormalizeValue(value);
        if (_value.EqualsColor(value)) {
            UpdateDisplayText();
            return false;
        }

        _value = value;
        UpdateDisplayText();
        if (notify)
            OnValueChanged();
        return true;
    }

    protected override void SetHexFormat(ColorHexFormat value) {
        if (_hexFormat == value) return;

        _hexFormat = value;
        if (!SetValue(_value, false))
            UpdateDisplayText();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, _value);

    protected override void UpdateDisplayText() => Text = HexPrefix + FormatValue(_value);

    protected override string FormatValue(Color32 value) => _hexFormat switch {
        ColorHexFormat.RGB => string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2:X2}", value.r, value.g, value.b),
        ColorHexFormat.RGBA => string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2:X2}{3:X2}", value.r, value.g, value.b, value.a),
        ColorHexFormat.ARGB => string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2:X2}{3:X2}", value.a, value.r, value.g, value.b),
        _ => "000000"
    };

    protected override bool TryParseValue(string text, out Color32 value) {
        value = default;

        if (string.IsNullOrEmpty(text) || !text.StartsWith(HexPrefix)) return false;

        text = text.Substring(1);
        if (!IsValidHex(text)) return false;

        try {
            switch (_hexFormat) {
                case ColorHexFormat.RGB: {
                    if (text.Length != 6) return false;

                    value = new Color32(
                        ParseHexByte(text, 0),
                        ParseHexByte(text, 2),
                        ParseHexByte(text, 4),
                        255
                    );
                    return true;
                }

                case ColorHexFormat.RGBA: {
                    if (text.Length != 8) return false;

                    value = new Color32(
                        ParseHexByte(text, 0),
                        ParseHexByte(text, 2),
                        ParseHexByte(text, 4),
                        ParseHexByte(text, 6)
                    );
                    return true;
                }


                case ColorHexFormat.ARGB: {
                    if (text.Length != 8) return false;

                    value = new Color32(
                        ParseHexByte(text, 2),
                        ParseHexByte(text, 4),
                        ParseHexByte(text, 6),
                        ParseHexByte(text, 0)
                    );
                    return true;
                }
            }
        }
        catch {
            return false;
        }

        return false;
    }

    protected virtual Color32 NormalizeValue(Color32 value) {
        if (_hexFormat == ColorHexFormat.RGB) {
            value.a = 255;
        }

        return value;
    }

    protected virtual bool IsValidHex(string text) {
        for (var i = 0; i < text.Length; i++) {
            var c = text[i];

            if (c is (< '0' or > '9') and (< 'A' or > 'F') and (< 'a' or > 'f')) {
                return false;
            }
        }

        return true;
    }

    protected byte ParseHexByte(string text, int index) => (byte)((ParseHexChar(text[index]) << 4) | ParseHexChar(text[index + 1]));

    protected int ParseHexChar(char c) => c switch {
        >= '0' and <= '9' => c - '0',
        >= 'A' and <= 'F' => c - 'A' + 10,
        >= 'a' and <= 'f' => c - 'a' + 10,
        _ => 0
    };
}