using System.Globalization;

namespace CSLModdingFramework.Lib.Formatting;

public sealed class DoubleFormatter : ValueFormatter<double> {
    public DoubleFormatter() : base("G") { }

    public DoubleFormatter(string formatString) : base(formatString) { }

    public override string Format(double value) => value.ToString(FormatString, CultureInfo.InvariantCulture);

    public override bool TryParse(string text, out double value) => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
