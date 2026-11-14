using CSLModdingFramework.Serialization;
using UnityEngine;

namespace CSLModdingFramework.Core;

public sealed class UpdateSystem : SystemBase {
    internal PhaseGroup<ISerializable> _serializationGroup;

    private UpdateSystemHost _host;

    protected override void OnCreate() {
        base.OnCreate();

        _serializationGroup = new PhaseGroup<ISerializable>();
        _host = UpdateSystemHost.Create(GetType().FullName);
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        Object.Destroy(_host);
    }

    public T UpdateBefore<T>(UpdatePhase phase) where T : SystemBase, new()
        => Register<T>(phase, UpdatePosition.Before);

    public T UpdateAt<T>(UpdatePhase phase) where T : SystemBase, new()
        => Register<T>(phase, UpdatePosition.At);

    public T UpdateAfter<T>(UpdatePhase phase) where T : SystemBase, new()
        => Register<T>(phase, UpdatePosition.After);

    private T Register<T>(UpdatePhase phase, UpdatePosition pos) where T : SystemBase, new() {
        var system = Domain.GetOrCreateSystem<T>();

        if (phase is UpdatePhase.Update) {
            _host.AddSystem(system, pos);
        }
        
        if (phase is UpdatePhase.Serialization) {
            var ser = system as ISerializable;
            if (ser != null) {
                switch (pos) {
                    case UpdatePosition.Before:
                        _serializationGroup.Before.Add(ser); break;
                    case UpdatePosition.At:
                        _serializationGroup.At.Add(ser); break;
                    case UpdatePosition.After:
                        _serializationGroup.After.Add(ser); break;
                    default:
                        _serializationGroup.At.Add(ser); break;
                }
            }
        }

        return system;
    }
}