using System;
using System.Globalization;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class FloatField : NumericField<float> {
    protected string _valueFormat = "F2";
    protected int _normalizeDecimalPlaces = 2;

    public virtual string ValueFormat {
        get => _valueFormat;
        set {
            if (string.Equals(value, _valueFormat)) return;
            _valueFormat = value;
            UpdateDisplayText();
        }
    }

    public virtual int NormalizeDecimalPlaces {
        get => _normalizeDecimalPlaces;
        set {
            value = Mathf.Max(0, value);

            if (value == _normalizeDecimalPlaces)
                return;

            _normalizeDecimalPlaces = value;
            Value = _value;
        }
    }

    protected override bool TryParseValue(string text, out float value) => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    protected override void IncreaseValue(float step) => Value += step;

    protected override void DecreaseValue(float step) => Value -= step;

    protected override string FormatValue(float value) => string.IsNullOrEmpty(_valueFormat) ? value.ToString(CultureInfo.InvariantCulture) : value.ToString(_valueFormat, CultureInfo.InvariantCulture);

    protected override float NormalizeValue(float value) => EnableNormalization ? (float)Math.Round(value, NormalizeDecimalPlaces, MidpointRounding.AwayFromZero) : value;
}