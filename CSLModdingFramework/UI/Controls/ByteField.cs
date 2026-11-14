using System;
using System.Globalization;

namespace CSLModdingFramework.UI.Controls;

public class ByteField : NumericField<byte> {
    protected string _valueFormat = "0";

    public string ValueFormat {
        get => _valueFormat;
        set {
            if (_valueFormat == value) return;

            _valueFormat = value;
            UpdateDisplayText();
        }
    }

    public override void Awake() {
        base.Awake();

        _useMinValue = true;
        _useMaxValue = true;

        _minValue = byte.MinValue;
        _maxValue = byte.MaxValue;
    }

    protected override bool TryParseValue(string text, out byte value) => byte.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

    protected override void IncreaseValue(byte step) => Value += step;

    protected override void DecreaseValue(byte step) => Value = (byte)Math.Max(byte.MinValue, Value - step);

    protected override string FormatValue(byte value) => string.IsNullOrEmpty(_valueFormat)
        ? value.ToString(CultureInfo.InvariantCulture)
        : value.ToString(_valueFormat, CultureInfo.InvariantCulture);
}