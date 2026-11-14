using ColossalFramework;
using UnityEngine;

namespace CSLModdingFramework.UI.Animations;

public struct LiteFloatAnimation {
    private EasingType _easing;

    public bool IsRunning => Elapsed < Duration;
    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public float Progress => Duration <= 0f ? 1f : Mathf.Min(Elapsed / Duration, 1f);
    public float StartValue { get; private set; }
    public float EndValue { get; private set; }

    public float Value {
        get {
            var t = Easing.Evaluate(_easing, Progress);
            return StartValue + (EndValue - StartValue) * t;
        }
    }

    public void UpdateTarget(float newTarget) => EndValue = newTarget;
    public void Cancel() => Elapsed = Duration;

    public void Start(float from, float to, float duration, EasingType easing) {
        StartValue = from;
        EndValue = to;
        Duration = Mathf.Max(0f, duration);
        Elapsed = 0f;
        _easing = easing;
    }

    public void StartTo(float to, float duration, EasingType easing) {
        StartValue = Value;
        EndValue = to;
        Duration = duration;
        Elapsed = 0f;
        _easing = easing;
    }

    public void Update(float deltaTime) {
        if (!IsRunning) return;
        Elapsed += Mathf.Max(0f, deltaTime);
    }

    public void JumpTo(float value) {
        StartValue = value;
        EndValue = value;
        Elapsed = Duration;
    }
}