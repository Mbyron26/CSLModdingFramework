using System.Globalization;

namespace CSLModdingFramework.Lib.Formatting;

public sealed class IntFormatter : ValueFormatter<int> {
    public IntFormatter() : base("G") { }

    public IntFormatter(string formatString) : base(formatString) { }

    public override string Format(int value) => value.ToString(FormatString, CultureInfo.InvariantCulture);

    public override bool TryParse(string text, out int value) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
}
