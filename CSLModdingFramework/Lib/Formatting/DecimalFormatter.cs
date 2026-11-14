using System.Globalization;

namespace CSLModdingFramework.Lib.Formatting;

public sealed class DecimalFormatter : ValueFormatter<decimal> {
    public DecimalFormatter() : base("G") { }

    public DecimalFormatter(string formatString) : base(formatString) { }

    public override string Format(decimal value) => value.ToString(FormatString, CultureInfo.InvariantCulture);

    public override bool TryParse(string text, out decimal value) => decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
