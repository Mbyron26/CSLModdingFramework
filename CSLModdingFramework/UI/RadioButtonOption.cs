using System;

namespace CSLModdingFramework.UI;

public class RadioButtonOption<T> {
    public int Id { get; internal set; }
    public string Header { get; internal set; }
    public string Description { get; internal set; }
    public bool IsSelected { get; internal set; }
    public bool IsEnabled { get; internal set; } = true;
    public bool IsVisible { get; internal set; } = true;
    internal Action<T, bool> SelectionCallback { get; set; }
    public T Value { get; }
    
    public RadioButtonOption(T value) => Value = value;

    internal RadioButtonOption(int id, T value) {
        Id = id;
        Value = value;
    }
}