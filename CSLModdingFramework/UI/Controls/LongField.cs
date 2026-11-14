using System.Globalization;

namespace CSLModdingFramework.UI.Controls;

public class LongField : NumericField<long> {
    protected string _valueFormat = "0";

    public virtual string ValueFormat {
        get => _valueFormat;
        set {
            if (string.Equals(value, _valueFormat))
                return;

            _valueFormat = value;
            UpdateDisplayText();
        }
    }

    protected override bool TryParseValue(string text, out long value) => long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

    protected override void IncreaseValue(long step) => Value += step;

    protected override void DecreaseValue(long step) => Value -= step;

    protected override string FormatValue(long value) => string.IsNullOrEmpty(_valueFormat)
        ? value.ToString(CultureInfo.InvariantCulture)
        : value.ToString(_valueFormat, CultureInfo.InvariantCulture);
}