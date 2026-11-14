using System.Collections.Generic;
using System.Linq;

namespace CSLModdingFramework.Extensions;

public static class EnumerableExtensions {
    public static string Join(this IEnumerable<string> values, string separator) => string.Join(separator, values.ToArray());
}