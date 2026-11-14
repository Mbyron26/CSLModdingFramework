using ColossalFramework;
using UnityEngine;

namespace CSLModdingFramework.UI.Animations;

public sealed class Vector3Animation : AnimationBase<Vector3> {
    protected override Vector3 Lerp(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);

    public void Start(Vector3 from, Vector3 to, float duration, EasingType easing) {
        StartValue = from;
        EndValue = to;
        Duration = Mathf.Max(0f, duration);
        Elapsed = 0f;
        EasingType = easing;
    }

    public void StartTo(Vector3 to, float duration, EasingType easing) {
        StartValue = Value;
        EndValue = to;
        Duration = Mathf.Max(0f, duration);
        Elapsed = 0f;
        EasingType = easing;
    }

    public void UpdateTarget(Vector3 newTarget) => EndValue = newTarget;
}