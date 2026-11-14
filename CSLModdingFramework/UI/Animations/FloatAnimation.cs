using ColossalFramework;
using UnityEngine;

namespace CSLModdingFramework.UI.Animations;

public sealed class FloatAnimation : AnimationBase<float> {
    protected override float Lerp(float startValue, float endValue, float t) => Mathf.Lerp(startValue, endValue, t);

    public void Start(float from, float to, float duration, EasingType easing) {
        StartValue = from;
        EndValue = to;
        Duration = Mathf.Max(0f, duration);
        Elapsed = 0f;
        EasingType = easing;
    }

    public void StartTo(float to, float duration, EasingType easing) {
        StartValue = Value;
        EndValue = to;
        Duration = Mathf.Max(0f, duration);
        Elapsed = 0f;
        EasingType = easing;
    }

    public void UpdateTarget(float newTarget) => EndValue = newTarget;
}