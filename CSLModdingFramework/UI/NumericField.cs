using System;
using System.Text;
using ColossalFramework.UI;
using CSLModdingFramework.Localization;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class NumericField<T> : ValueField, INumericField<T> where T : struct, IComparable<T> {
    protected bool _mouseWheelEnabled;
    protected T _value;
    protected T _minValue;
    protected T _maxValue;
    protected T _stepSize;
    protected T _shiftStepSize;
    protected T _controlStepSize;
    protected bool _useMinValue;
    protected bool _useMaxValue;
    protected string _valuePrefix;
    protected string _valueSuffix;
    protected bool _enableFormatting;
    protected bool _enableNormalization;
    protected StringBuilder _stringBuilder;
    protected bool _showTooltip = true;

    public event UIEventHandler<ValueField, T> FieldValueChanged;
    public event Action<T> ValueChanged;

    public virtual T Value {
        get => _value;
        set => SetValue(value);
    }

    public virtual bool UseMinValue {
        get => _useMinValue;
        set {
            if (value.Equals(_useMinValue)) return;

            _useMinValue = value;
            UpdateDisplayText();
        }
    }

    public virtual bool UseMaxValue {
        get => _useMaxValue;
        set {
            if (value.Equals(_useMaxValue)) return;

            _useMaxValue = value;
            UpdateDisplayText();
        }
    }

    public virtual T MinValue {
        get => _minValue;
        set {
            if (value.Equals(_minValue)) return;

            _minValue = value;
            UpdateDisplayText();
        }
    }

    public virtual T MaxValue {
        get => _maxValue;
        set {
            if (value.Equals(_maxValue)) return;

            _maxValue = value;
            UpdateDisplayText();
        }
    }

    public virtual T StepSize {
        get => _stepSize;
        set {
            if (value.Equals(_stepSize)) return;

            _stepSize = value;
            UpdateDisplayText();
        }
    }

    public virtual T ControlStepSize {
        get => _controlStepSize;
        set {
            if (value.Equals(_controlStepSize)) return;

            _controlStepSize = value;
            UpdateDisplayText();
        }
    }

    public virtual T ShiftStepSize {
        get => _shiftStepSize;
        set {
            if (value.Equals(_shiftStepSize)) return;

            _shiftStepSize = value;
            UpdateDisplayText();
        }
    }

    public virtual bool EnableFormatting {
        get => _enableFormatting;
        set {
            if (value.Equals(_enableFormatting)) return;

            _enableFormatting = value;
            UpdateDisplayText();
        }
    }

    public virtual bool EnableNormalization {
        get => _enableNormalization;
        set {
            if (value.Equals(_enableNormalization)) return;

            _enableNormalization = value;
            Value = _value;
        }
    }

    public virtual string ValuePrefix {
        get => _valuePrefix;
        set {
            if (string.Equals(value, _valuePrefix)) return;

            _valuePrefix = value;
            UpdateDisplayText();
        }
    }

    public virtual string ValueSuffix {
        get => _valueSuffix;
        set {
            if (string.Equals(value, _valueSuffix)) return;

            _valueSuffix = value;
            UpdateDisplayText();
        }
    }

    public virtual bool MouseWheelEnabled {
        get => _mouseWheelEnabled;
        set {
            if (_mouseWheelEnabled == value) return;

            _mouseWheelEnabled = value;
            UpdateTooltip();
        }
    }

    public override TextSizeMode SizeMode => _sizeMode;

    public bool ShowTooltip {
        get => _showTooltip;
        set {
            if (_showTooltip == value) return;

            _showTooltip = value;
            UpdateTooltip();
        }
    }

    public ILocalizationProvider LocalizationProvider { get; set; }

    protected abstract bool TryParseValue(string text, out T value);
    protected abstract void IncreaseValue(T step);
    protected abstract void DecreaseValue(T step);

    public virtual bool SetValueWithoutNotify(T value) => SetValue(value, false);

    public virtual bool SetValue(T value, bool notify = true) {
        value = ProcessValue(value);

        if (_value.Equals(value)) return false;

        _value = value;
        UpdateDisplayText();

        if (notify) {
            NotifyFieldValueChanged(this, _value);
            NotifyValueChanged(_value);
        }

        return true;
    }

    protected void NotifyValueChanged(T value) => ValueChanged?.Invoke(value);
    protected void NotifyFieldValueChanged(ValueField sender, T value) => FieldValueChanged?.Invoke(sender, value);

    protected virtual T NormalizeValue(T value) => value;
    protected virtual string FormatValue(T value) => value.ToString();

    protected virtual string FormatDisplayText(T value) {
        var text = _enableFormatting
            ? FormatValue(value)
            : value.ToString();

        if (!string.IsNullOrEmpty(ValuePrefix))
            text = ValuePrefix + text;

        if (!string.IsNullOrEmpty(ValueSuffix))
            text += ValueSuffix;

        return text;
    }

    protected virtual void UpdateTooltip() => tooltip = _mouseWheelEnabled && _showTooltip ? GetTooltipString() : string.Empty;

    protected virtual string GetTooltipString() {
        if (_stringBuilder == null) {
            _stringBuilder = new StringBuilder();
        }
        else {
            _stringBuilder.Remove(0, _stringBuilder.Length);
        }

        var provider = LocalizationProvider;

        _stringBuilder.AppendLine(
            string.Format(
                provider != null
                    ? provider.GetString("WheelStep")
                    : "Wheel Step: {0}",
                _stepSize));

        _stringBuilder.AppendLine(
            string.Format(
                provider != null
                    ? provider.GetString("ShiftStep")
                    : "Shift Step: {0}",
                _shiftStepSize));

        _stringBuilder.Append(
            string.Format(
                provider != null
                    ? provider.GetString("ControlStep")
                    : "Control Step: {0}",
                _controlStepSize));

        return _stringBuilder.ToString();
    }

    protected virtual T ClampValue(T value) {
        if (_useMinValue && value.CompareTo(_minValue) < 0)
            value = _minValue;

        if (_useMaxValue && value.CompareTo(_maxValue) > 0)
            value = _maxValue;

        return value;
    }

    protected virtual T ProcessValue(T value) {
        value = ClampValue(value);

        if (EnableNormalization)
            value = NormalizeValue(value);

        return value;
    }

    protected virtual void UpdateDisplayText() => Text = FormatDisplayText(_value);

    protected override void OnLostFocusInternal(UIFocusEventParameter p) {
        _imeActive = false;
        Input.imeCompositionMode = IMECompositionMode.Auto;
        _cursorBlinkElapsed = 0f;

        if (_cursorShown) {
            _cursorShown = false;
        }

        ClearSelection();
        UpdateTooltip();

        if (TryParseValue(_text, out var value)) {
            if (!SetValue(value)) {
                UpdateDisplayText();
            }
        }
        else {
            UpdateDisplayText();
        }

        Invalidate();
    }

    protected override void OnGotFocusInternal(UIFocusEventParameter p) {
        _imeActive = true;
        Input.imeCompositionMode = IMECompositionMode.On;
        if (SelectAllOnFocus && !string.IsNullOrEmpty(_text)) {
            SetSelection(0, _text.Length);
        }

        _cursorBlinkElapsed = 0f;
        if (!_cursorShown) {
            _cursorShown = true;
        }

        Invalidate();

        tooltip = string.Empty;
        if (tooltipBox != null)
            tooltipBox.Hide();
    }

    protected override void OnMouseDownInternal(UIMouseEventParameter p) {
        // if (!hasFocus) {
        //     Text = _enableFormatting ? FormatValue(_value) : _value.ToString();
        // }
        // else {
        //     Text = _value.ToString();
        // }
        Text = _value.ToString();

        base.OnMouseDownInternal(p);
    }

    protected override void OnMouseWheel(UIMouseEventParameter p) {
        base.OnMouseWheel(p);
        
        Unfocus();

        if (tooltipBox != null)
            tooltipBox.Hide();

        if (p.used || !m_IsMouseHovering || !MouseWheelEnabled) return;

        p.Use();
        T step;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            step = _shiftStepSize;
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            step = _controlStepSize;
        else
            step = _stepSize;

        if (p.wheelDelta > 0)
            IncreaseValue(step);
        else
            DecreaseValue(step);
    }
}