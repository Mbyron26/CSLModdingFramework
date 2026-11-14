using System.Collections.Generic;
using UnityEngine;

namespace CSLModdingFramework.Core;

internal sealed class UpdateSystemHost : MonoBehaviour {
    private readonly List<(SystemBase system, UpdatePosition pos)> _pendingAdd = new();
    private readonly HashSet<SystemBase> _allSystems = new();

    private PhaseGroup<SystemBase> _updateGroup;
    private bool _isUpdating;
    private uint _frameIndex;
    private bool _hostStarted;

    public static UpdateSystemHost Create(string name) {
        var go = new GameObject($"UpdateSystemHost_{name}");
        var host = go.AddComponent<UpdateSystemHost>();
        return host;
    }

    public void AddSystem(SystemBase system, UpdatePosition pos) {
        if (!_allSystems.Add(system))
            return;

        if (_isUpdating)
            _pendingAdd.Add((system, pos));
        else
            InsertSystem(system, pos);
    }

    private void InsertSystem(SystemBase sys, UpdatePosition pos) {
        switch (pos) {
            case UpdatePosition.Before:
                _updateGroup.Before.Add(sys);
                break;

            case UpdatePosition.At:
                _updateGroup.At.Add(sys);
                break;

            case UpdatePosition.After:
                _updateGroup.After.Add(sys);
                break;
        }

        sys.OnStartRunning();
    }

    private void FlushPending() {
        if (_pendingAdd.Count == 0)
            return;

        for (var i = 0; i < _pendingAdd.Count; i++) {
            var (sys, pos) = _pendingAdd[i];
            InsertSystem(sys, pos);
        }

        _pendingAdd.Clear();
    }

    private void RunUpdateInternal(List<SystemBase> list) {
        for (var i = 0; i < list.Count; i++) {
            var sys = list[i];
            if (!sys.UpdateEnabled)
                continue;

            var interval = sys.UpdateInterval();
            if (interval <= 1) {
                sys.OnUpdate();
                continue;
            }

            if (_frameIndex % interval == 0) {
                sys.OnUpdate();
            }
        }
    }

    private void Awake() => _updateGroup = new PhaseGroup<SystemBase>();

    private void OnEnable() {
        if (_hostStarted) {
            CallStartOnAllSystems();
        }

        _hostStarted = true;
    }

    private void OnDisable() => CallStopOnAllSystems();

    private void Update() {
        _isUpdating = true;

        RunUpdateInternal(_updateGroup.Before);
        RunUpdateInternal(_updateGroup.At);
        RunUpdateInternal(_updateGroup.After);

        _isUpdating = false;

        FlushPending();
        _frameIndex++;
    }

    private void CallStopOnAllSystems() {
        foreach (var sys in _updateGroup.Before)
            sys.OnStopRunning();

        foreach (var sys in _updateGroup.At)
            sys.OnStopRunning();

        foreach (var sys in _updateGroup.After)
            sys.OnStopRunning();
    }

    private void CallStartOnAllSystems() {
        foreach (var sys in _updateGroup.Before)
            sys.OnStartRunning();

        foreach (var sys in _updateGroup.At)
            sys.OnStartRunning();

        foreach (var sys in _updateGroup.After)
            sys.OnStartRunning();
    }
}