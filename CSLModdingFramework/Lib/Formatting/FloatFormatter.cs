using System.Globalization;

namespace CSLModdingFramework.Lib.Formatting;

public sealed class FloatFormatter : ValueFormatter<float> {
    public FloatFormatter() : base("G") { }

    public FloatFormatter(string formatString) : base(formatString) { }

    public override string Format(float value) => value.ToString(FormatString, CultureInfo.InvariantCulture);

    public override bool TryParse(string text, out float value) => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
