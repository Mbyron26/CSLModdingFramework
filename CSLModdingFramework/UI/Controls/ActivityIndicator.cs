using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class ActivityIndicator : SpriteView {
    private float _spinSpeed = 180f;

    public bool IsSpinning { get; private set; }

    public float SpinSpeed {
        get => _spinSpeed;
        set {
            _spinSpeed = value;
            Invalidate();
        }
    }

    public override void Awake() {
        base.Awake();
        size = new Vector2(20, 20);
        pivot = UIPivotPoint.MiddleCenter;
    }

    public override void Update() {
        base.Update();
        if (isVisible && IsSpinning)
            transform.Rotate(Vector3.forward, _spinSpeed * Time.deltaTime);
    }

    public void Play() => IsSpinning = true;

    public void Stop() => IsSpinning = false;
}