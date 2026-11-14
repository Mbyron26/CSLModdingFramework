using System;

namespace CSLModdingFramework.UI;

public interface INumericField<T> where T : struct, IComparable<T> {
    bool MouseWheelEnabled { get; set; }
    T Value { get; set; }
    T MinValue { get; set; }
    T MaxValue { get; set; }
    T StepSize { get; set; }
    T ShiftStepSize { get; set; }
    T ControlStepSize { get; set; }
    bool UseMinValue { get; set; }
    bool UseMaxValue { get; set; }
    string ValuePrefix { get; set; }
    string ValueSuffix { get; set; }
    bool EnableFormatting { get; set; }
    bool EnableNormalization { get; set; }
}