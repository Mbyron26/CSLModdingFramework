using CSLModdingFramework.Collections;

namespace CSLModdingFramework.Core.Compatibility;

public interface IIncompatibleModRule {
    IReadOnlyDictionary<string, IncompatibleModItem> Lookup { get; }

    IncompatibleModRule Add(IncompatibleModItem incompatibleModItem);
    IncompatibleModRule Add(string assemblyName, IncompatibilityModLevel incompatibilityLevel, string displayName, bool isDuplicateFunctionality = true, string alternativeModName = null, string customWarningMessage = null);
}