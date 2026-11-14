using ColossalFramework;
using UnityEngine;

namespace CSLModdingFramework.UI.Animations;

public sealed class Color32Animation : AnimationBase<Color32> {
    protected override Color32 Lerp(Color32 a, Color32 b, float t) => new(
        (byte)Mathf.Lerp(a.r, b.r, t),
        (byte)Mathf.Lerp(a.g, b.g, t),
        (byte)Mathf.Lerp(a.b, b.b, t),
        (byte)Mathf.Lerp(a.a, b.a, t)
    );

    public void Start(Color32 from, Color32 to, float duration, EasingType easing) {
        StartValue = from;
        EndValue = to;
        Duration = Mathf.Max(0f, duration);
        Elapsed = 0f;
        EasingType = easing;
    }

    public void StartTo(Color32 to, float duration, EasingType easing) {
        StartValue = Value;
        EndValue = to;
        Duration = Mathf.Max(0f, duration);
        Elapsed = 0f;
        EasingType = easing;
    }

    public void UpdateTarget(Color32 newTarget) => EndValue = newTarget;
}