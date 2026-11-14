using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class Slider : SliderBase {
    public event UIEventHandler<Slider, float> SliderValueChanged;

    public override void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        _trackRenderPosition = new Vector3(_padding.Left, _padding.Top);

        IsPerformingAutoLayout = false;
    }

    public override void SetValue(float value, bool notify = true) {
        var v = Mathf.Clamp(value, _minValue, _maxValue);

        if (_stepSize > 0f) {
            var steps = Mathf.Round((v - _minValue) / _stepSize);
            v = _minValue + steps * _stepSize;
            v = Mathf.Clamp(v, _minValue, _maxValue);
        }

        if (Mathf.Approximately(_value, v)) return;

        _value = v;

        if (!Mathf.Approximately(_maxValue, _minValue))
            _normalizedValue = (v - _minValue) / (_maxValue - _minValue);
        else
            _normalizedValue = 0f;

        Invalidate();

        if (notify) {
            SliderValueChanged?.Invoke(this, v);
            NotifyValueChanged(v);
        }
    }

    protected override void OnKeyDown(UIKeyEventParameter p) {
        if (!EnableArrowKeys || p.used) {
            base.OnKeyDown(p);
            return;
        }

        var delta = 0f;

        if (Direction == Orientation.Horizontal) {
            if (p.keycode == KeyCode.LeftArrow)
                delta = -ArrowKeyStep;

            else if (p.keycode == KeyCode.RightArrow)
                delta = ArrowKeyStep;
        }
        else {
            if (p.keycode == KeyCode.UpArrow)
                delta = -ArrowKeyStep;

            else if (p.keycode == KeyCode.DownArrow)
                delta = ArrowKeyStep;
        }

        if (delta != 0f) {
            var multiplier = 1f;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                multiplier *= ArrowShiftMultiplier;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                multiplier *= ArrowCtrlMultiplier;

            Value += delta * multiplier;
            p.Use();
            return;
        }

        base.OnKeyDown(p);
    }

    protected override void OnRebuildRenderData() {
        if (IsAutoLayoutSuspended) return;

        RenderTrack();
        RenderFill();
        RenderThumb();
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended) {
            PerformAutoLayout();
        }

        base.OnSizeChanged();
    }
}