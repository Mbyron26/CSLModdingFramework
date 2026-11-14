using ICities;
using System;
using CSLModdingFramework.Core;

namespace CSLModdingFramework.Extensions;

public static class LoadModeExtensions {
    public static LoadContext ToLoadContext(this LoadMode mode) => mode switch {
        LoadMode.NewGame => new LoadContext(LoadTarget.Game, LoadSource.None, LoadAction.New),
        LoadMode.LoadGame => new LoadContext(LoadTarget.Game, LoadSource.None, LoadAction.Load),
        LoadMode.NewMap => new LoadContext(LoadTarget.Map, LoadSource.None, LoadAction.New),
        LoadMode.LoadMap => new LoadContext(LoadTarget.Map, LoadSource.None, LoadAction.Load),
        LoadMode.NewAsset => new LoadContext(LoadTarget.Asset, LoadSource.None, LoadAction.New),
        LoadMode.LoadAsset => new LoadContext(LoadTarget.Asset, LoadSource.None, LoadAction.Load),
        LoadMode.NewTheme => new LoadContext(LoadTarget.Theme, LoadSource.None, LoadAction.New),
        LoadMode.LoadTheme => new LoadContext(LoadTarget.Theme, LoadSource.None, LoadAction.Load),
        LoadMode.NewScenarioFromGame => new LoadContext(LoadTarget.Scenario, LoadSource.FromGame, LoadAction.New),
        LoadMode.NewScenarioFromMap => new LoadContext(LoadTarget.Scenario, LoadSource.FromMap, LoadAction.New),
        LoadMode.LoadScenario => new LoadContext(LoadTarget.Scenario, LoadSource.None, LoadAction.Load),
        LoadMode.NewGameFromScenario => new LoadContext(LoadTarget.Game, LoadSource.FromScenario, LoadAction.New),
        LoadMode.UpdateScenarioFromGame => new LoadContext(LoadTarget.Scenario, LoadSource.FromGame, LoadAction.Update),
        LoadMode.UpdateScenarioFromMap => new LoadContext(LoadTarget.Scenario, LoadSource.FromMap, LoadAction.Update),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
    };
}
