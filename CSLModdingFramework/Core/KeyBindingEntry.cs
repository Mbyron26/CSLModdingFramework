using System;

namespace CSLModdingFramework.Core;

public sealed class KeyBindingEntry {
    public string Id { get; }
    public KeyBinding Binding { get; }
    public Action Action { get; }
    public KeyBindingContext Context { get; }
    
    public KeyBindingEntry(string id, KeyBinding binding, Action action, KeyBindingContext context) {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Binding = binding;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Context = context;
    }
}