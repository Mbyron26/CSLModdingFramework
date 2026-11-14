using System.Globalization;

namespace CSLModdingFramework.UI.Controls;

public class IntField : NumericField<int> {
    protected string _valueFormat = "0";

    public string ValueFormat {
        get => _valueFormat;
        set {
            if (_valueFormat == value) return;

            _valueFormat = value;
            UpdateDisplayText();
        }
    }

    protected override bool TryParseValue(string text, out int value) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

    protected override void IncreaseValue(int step) => Value += step;

    protected override void DecreaseValue(int step) => Value -= step;

    protected override string FormatValue(int value) => string.IsNullOrEmpty(_valueFormat)
        ? value.ToString(CultureInfo.InvariantCulture)
        : value.ToString(_valueFormat, CultureInfo.InvariantCulture);
}