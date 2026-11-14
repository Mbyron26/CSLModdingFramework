using ColossalFramework;
using UnityEngine;

namespace CSLModdingFramework.UI.Animations;

public abstract class AnimationBase<T> {
    protected EasingFunctions.EasingFunction _easingFunc = EasingFunctions.GetFunction(EasingType.Linear);
    protected EasingType _easingType = EasingType.Linear;

    public bool IsRunning => Elapsed < Duration;
    public float Duration { get; protected set; }
    public float Elapsed { get; protected set; }
    public float Progress => Duration <= 0f ? 1f : Mathf.Clamp01(Elapsed / Duration);

    public T StartValue { get; protected set; }
    public T EndValue { get; protected set; }

    public EasingType EasingType {
        get => _easingType;
        set {
            _easingType = value;
            _easingFunc = EasingFunctions.GetFunction(value);
        }
    }

    public T Value {
        get {
            var t = _easingFunc(0f, 1f, Progress);
            return Lerp(StartValue, EndValue, t);
        }
    }

    protected abstract T Lerp(T startValue, T endValue, float t);

    public virtual void Update(float deltaTime) {
        if (!IsRunning) return;
        Elapsed = Mathf.Min(Duration, Elapsed + Mathf.Max(0f, deltaTime));
    }

    public virtual void Cancel() => Elapsed = Duration;

    public virtual void JumpTo(T value) {
        StartValue = value;
        EndValue = value;
        Elapsed = Duration;
    }
}