namespace CSLModdingFramework.Lib.Numeric;

public struct ValueChangedArgs<T> {
    public T OldValue;
    public T NewValue;

    public ValueChangedArgs() {
        OldValue = default;
        NewValue = default;
    }

    public ValueChangedArgs(T oldValue, T newValue) {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
