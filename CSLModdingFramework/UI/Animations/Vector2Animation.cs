using ColossalFramework;
using UnityEngine;

namespace CSLModdingFramework.UI.Animations;

public sealed class Vector2Animation : AnimationBase<Vector2> {
    protected override Vector2 Lerp(Vector2 a, Vector2 b, float t) => Vector2.Lerp(a, b, t);

    public void Start(Vector2 from, Vector2 to, float duration, EasingType easing) {
        StartValue = from;
        EndValue = to;
        Duration = Mathf.Max(0f, duration);
        Elapsed = 0f;
        EasingType = easing;
    }

    public void StartTo(Vector2 to, float duration, EasingType easing) {
        StartValue = Value;
        EndValue = to;
        Duration = Mathf.Max(0f, duration);
        Elapsed = 0f;
        EasingType = easing;
    }

    public void UpdateTarget(Vector2 newTarget) => EndValue = newTarget;
}