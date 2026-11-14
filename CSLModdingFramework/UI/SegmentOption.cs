using System;

namespace CSLModdingFramework.UI;

public class SegmentOption<T> {
    public int Id { get; internal set; }
    public string Text { get; internal set; }
    public string Icon { get; internal set; }
    public bool IsSelected { get; internal set; }
    public bool IsEnabled { get; internal set; } = true;
    public bool IsVisible { get; internal set; } = true;
    internal Action<T, bool> SelectionCallback { get; set; }
    public T Value { get; }

    public SegmentOption(T value) => Value = value;

    internal SegmentOption(int id, T value) {
        Id = id;
        Value = value;
    }
}