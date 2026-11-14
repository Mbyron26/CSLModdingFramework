using System.Globalization;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class ColorHexField : ColorHexFieldBase<Color> {
    protected Color _value = new(1f, 1f, 1f, 1f);
    
    public event UIEventHandler<ColorHexField, Color> ValueChanged;

    public virtual Color Value {
        get => _value;
        set => SetValue(value);
    }
    
    public override bool SetValueWithoutNotify(Color value) => SetValue(value, false);

    public override bool SetValue(Color value, bool notify = true) {
        value = NormalizeValue(value);

        if (ApproximatelyEqual(_value, value)) {
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

    protected override string FormatValue(Color value) {
        var r = FloatToByte(value.r);
        var g = FloatToByte(value.g);
        var b = FloatToByte(value.b);
        var a = FloatToByte(value.a);

        return _hexFormat switch {
            ColorHexFormat.RGB => string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2:X2}", r, g, b),
            ColorHexFormat.RGBA => string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a),
            ColorHexFormat.ARGB => string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2:X2}{3:X2}", a, r, g, b),
            _ => "000000"
        };
    }

    protected override bool TryParseValue(string text, out Color value) {
        value = default;

        if (string.IsNullOrEmpty(text) || !text.StartsWith(HexPrefix))
            return false;

        text = text.Substring(1);
        if (!IsValidHex(text))
            return false;


        switch (_hexFormat) {
            case ColorHexFormat.RGB: {
                if (text.Length != 6) return false;

                value = new Color(
                    ByteToFloat(ParseHexByte(text, 0)),
                    ByteToFloat(ParseHexByte(text, 2)),
                    ByteToFloat(ParseHexByte(text, 4)),
                    1f
                );
                return true;
            }

            case ColorHexFormat.RGBA: {
                if (text.Length != 8) return false;

                value = new Color(
                    ByteToFloat(ParseHexByte(text, 0)),
                    ByteToFloat(ParseHexByte(text, 2)),
                    ByteToFloat(ParseHexByte(text, 4)),
                    ByteToFloat(ParseHexByte(text, 6))
                );
                return true;
            }

            case ColorHexFormat.ARGB: {
                if (text.Length != 8) return false;

                value = new Color(
                    ByteToFloat(ParseHexByte(text, 2)),
                    ByteToFloat(ParseHexByte(text, 4)),
                    ByteToFloat(ParseHexByte(text, 6)),
                    ByteToFloat(ParseHexByte(text, 0))
                );
                return true;
            }
        }

        return false;
    }

    protected virtual Color NormalizeValue(Color value) {
        if (_hexFormat == ColorHexFormat.RGB) {
            value.a = 1f;
        }

        return value;
    }

    protected virtual bool IsValidHex(string text) {
        for (var i = 0; i < text.Length; i++) {
            var c = text[i];
            if (c is (< '0' or > '9') and (< 'A' or > 'F') and (< 'a' or > 'f'))
                return false;
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

    protected static byte FloatToByte(float f) => (byte)Mathf.Clamp(Mathf.RoundToInt(f * 255f), 0, 255);

    protected static float ByteToFloat(byte b) => b / 255f;

    protected static bool ApproximatelyEqual(Color a, Color b) =>
        FloatToByte(a.r) == FloatToByte(b.r)
        && FloatToByte(a.g) == FloatToByte(b.g)
        && FloatToByte(a.b) == FloatToByte(b.b)
        && FloatToByte(a.a) == FloatToByte(b.a);
}