using System.Globalization;
using CSLModdingFramework.Extensions;

namespace CSLModdingFramework.Lib.Formatting;

public sealed class PercentageFormatter : ValueFormatter<double> {
    public PercentageFormatter() : base("P2") { }

    public PercentageFormatter(string formatString) : base(formatString) { }

    public override string Format(double value) => value.ToString(FormatString, CultureInfo.InvariantCulture);

    public override bool TryParse(string text, out double value) {
        if (text.IsNullOrWhiteSpace()) {
            value = 0;
            return false;
        }

        text = text.Trim();

        if (!text.EndsWith("%"))
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        var numberPart = text.Substring(0, text.Length - 1).Trim();

        if (double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)) {
            value = parsed / 100.0;
            return true;
        }

        value = 0;
        return false;
    }
}