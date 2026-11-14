using System;
using System.Globalization;

namespace CSLModdingFramework.Lib.Formatting;

public sealed class DateTimeFormatter : ValueFormatter<DateTime> {
    public DateTimeFormatter() : base("yyyy-MM-dd HH:mm:ss") { }

    public DateTimeFormatter(string formatString) : base(formatString) { }

    public override string Format(DateTime value) => value.ToString(FormatString, CultureInfo.InvariantCulture);

    public override bool TryParse(string text, out DateTime value) => DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
}
