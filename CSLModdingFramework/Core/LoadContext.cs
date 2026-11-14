namespace CSLModdingFramework.Core;

public readonly struct LoadContext {
    public LoadTarget Target { get; }
    public LoadSource Source { get; }
    public LoadAction Action { get; }

    public LoadContext(LoadTarget target, LoadSource source, LoadAction action) {
        Target = target;
        Source = source;
        Action = action;
    }

    public bool IsNew => Action == LoadAction.New;
    public bool IsLoad => Action == LoadAction.Load;
    public bool IsUpdate => Action == LoadAction.Update;

    public bool IsGame => Target == LoadTarget.Game;
    public bool IsMap => Target == LoadTarget.Map;
    public bool IsScenario => Target == LoadTarget.Scenario;
    public bool IsAsset => Target == LoadTarget.Asset;

    public bool HasSource => Source != LoadSource.None;

    public bool IsFromGame => Source == LoadSource.FromGame;
    public bool IsFromMap => Source == LoadSource.FromMap;
    public bool IsFromScenario => Source == LoadSource.FromScenario;

    public override string ToString() => $"{Action} {Target} (Source: {Source})";
}
