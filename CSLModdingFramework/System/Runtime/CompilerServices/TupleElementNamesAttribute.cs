using System.Collections.Generic;

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public sealed class TupleElementNamesAttribute : Attribute {
    public TupleElementNamesAttribute(string[] transformNames) {
        TransformNames = transformNames;
    }

    public IList<string> TransformNames { get; }
}
