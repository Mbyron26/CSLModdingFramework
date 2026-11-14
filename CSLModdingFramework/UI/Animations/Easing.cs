using ColossalFramework;
using UnityEngine;

namespace CSLModdingFramework.UI.Animations;

public static class Easing {
    public static float Evaluate(EasingType type, float t) {
        t = Mathf.Clamp01(t);

        switch (type) {
            case EasingType.None:
                return 1f;

            case EasingType.Linear:
                return t;

            case EasingType.QuadEaseIn:
                return t * t;

            case EasingType.QuadEaseOut:
                return t * (2f - t);

            case EasingType.QuadEaseInOut:
                return t < 0.5f
                    ? 2f * t * t
                    : -1f + (4f - 2f * t) * t;

            case EasingType.CubicEaseIn:
                return t * t * t;

            case EasingType.CubicEaseOut:
                t -= 1f;
                return t * t * t + 1f;

            case EasingType.CubicEaseInOut:
                return t < 0.5f
                    ? 4f * t * t * t
                    : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;

            case EasingType.QuartEaseIn:
                return t * t * t * t;

            case EasingType.QuartEaseOut:
                t -= 1f;
                return 1f - t * t * t * t;

            case EasingType.QuartEaseInOut:
                if (t < 0.5f) {
                    return 8f * t * t * t * t;
                }
                t -= 1f;
                return 1f - 8f * t * t * t * t;

            case EasingType.QuintEaseIn:
                return t * t * t * t * t;

            case EasingType.QuintEaseOut:
                t -= 1f;
                return t * t * t * t * t + 1f;

            case EasingType.QuintEaseInOut:
                if (t < 0.5f) {
                    return 16f * t * t * t * t * t;
                }
                t -= 1f;
                return 1f + 16f * t * t * t * t * t;

            case EasingType.SineEaseIn:
                return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            case EasingType.SineEaseOut:
                return Mathf.Sin(t * Mathf.PI * 0.5f);

            case EasingType.SineEaseInOut:
                return 0.5f * (1f - Mathf.Cos(Mathf.PI * t));

            case EasingType.ExpoEaseIn:
                return t <= 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f));

            case EasingType.ExpoEaseOut:
                return t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);

            case EasingType.ExpoEaseInOut:
                if (t <= 0f) return 0f;
                if (t >= 1f) return 1f;
                if (t < 0.5f) {
                    return 0.5f * Mathf.Pow(2f, 20f * t - 10f);
                }
                return 1f - 0.5f * Mathf.Pow(2f, -20f * t + 10f);

            case EasingType.CircEaseIn:
                return 1f - Mathf.Sqrt(1f - t * t);

            case EasingType.CircEaseOut:
                t -= 1f;
                return Mathf.Sqrt(1f - t * t);

            case EasingType.CircEaseInOut:
                if (t < 0.5f) {
                    return 0.5f * (1f - Mathf.Sqrt(1f - 4f * t * t));
                }
                t = 2f * t - 1f;
                return 0.5f * (Mathf.Sqrt(1f - t * t) + 1f);

            case EasingType.Bounce:

                return BounceOut(t);

            case EasingType.BackEaseIn:
                return BackIn(t);

            case EasingType.BackEaseOut:
                return BackOut(t);

            case EasingType.BackEaseInOut:
                return BackInOut(t);

            case EasingType.Spring:
                return Spring(t);

            default:
                return t;
        }
    }

    private static float BounceOut(float t) {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1f / d1) {
            return n1 * t * t;
        } else if (t < 2f / d1) {
            t -= 1.5f / d1;
            return n1 * t * t + 0.75f;
        } else if (t < 2.5f / d1) {
            t -= 2.25f / d1;
            return n1 * t * t + 0.9375f;
        } else {
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }

    private static float BackIn(float t) {
        const float s = 1.70158f;
        return t * t * ((s + 1f) * t - s);
    }

    private static float BackOut(float t) {
        const float s = 1.70158f;
        t -= 1f;
        return t * t * ((s + 1f) * t + s) + 1f;
    }

    private static float BackInOut(float t) {
        float s = 1.70158f * 1.525f;
        t *= 2f;
        if (t < 1f) {
            return 0.5f * (t * t * ((s + 1f) * t - s));
        }
        t -= 2f;
        return 0.5f * (t * t * ((s + 1f) * t + s) + 2f);
    }

    private static float Spring(float t) {

        t = Mathf.Clamp01(t);
        return (Mathf.Sin(t * Mathf.PI * (0.2f + 2.5f * t * t * t)) *
                Mathf.Pow(1f - t, 2.2f) + t) *
               (1f + 1.2f * (1f - t));
    }
}