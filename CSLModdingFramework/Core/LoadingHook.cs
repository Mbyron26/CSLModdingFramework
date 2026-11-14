using System;
using ICities;

namespace CSLModdingFramework.Core;

public class LoadingHook : ILoadingExtension {
    internal static event Action GamePreLoad;
    internal static event Action<LoadMode> GameLoaded;
    internal static event Action GamePreUnload;
    internal static event Action GameUnloaded;

    public void OnCreated(ILoading loading) => GamePreLoad?.Invoke();

    public void OnLevelLoaded(LoadMode mode) => GameLoaded?.Invoke(mode);

    public void OnLevelUnloading() => GamePreUnload?.Invoke();

    public void OnReleased() => GameUnloaded?.Invoke();
}
