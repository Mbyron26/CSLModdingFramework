namespace CSLModdingFramework.Core;

public class KeyBinding {
    public KeyCombination Combination { get; set; }
    public KeyCombination DefaultCombination { get; }
    public KeyBindingTriggerMode TriggerMode { get; set; }
    public bool Enabled { get; set; } = true;

    public KeyBinding(KeyCombination combination, KeyBindingTriggerMode triggerMode = KeyBindingTriggerMode.Down) {
        DefaultCombination = Combination = combination;
        TriggerMode = triggerMode;
    }

    public bool IsTriggered() {
        if (!Enabled) return false;

        return TriggerMode switch {
            KeyBindingTriggerMode.Down => Combination.IsDown(),
            KeyBindingTriggerMode.Hold => Combination.IsPressed(),
            KeyBindingTriggerMode.Up => Combination.IsUp(),
            _ => false
        };
    }

    public void Reset() => Combination = DefaultCombination;

    public string GetDisplayString() => Combination.ToString();

    public bool HasConflictWith(KeyBinding other) => other != null && Combination == other.Combination;

    public override string ToString() => Combination.ToString();
}