using ColossalFramework.UI;

namespace CSLModdingFramework.UI.Rendering;

public abstract class StateValue<T> : IAutoRender {
    protected T _normal;
    protected T _hovered;
    protected T _pressed;
    protected T _focused;
    protected T _disabled;
    protected int _renderSuspendCount;

    public UIComponent Parent { get; set; }
    public bool IsPerformingAutoRender { get; protected set; }
    public bool IsAutoRenderSuspended => _renderSuspendCount > 0;

    public T Normal {
        get => _normal;
        set => SetValue(ref _normal, value);
    }

    public T Hovered {
        get => _hovered;
        set => SetValue(ref _hovered, value);
    }

    public T Pressed {
        get => _pressed;
        set => SetValue(ref _pressed, value);
    }

    public T Focused {
        get => _focused;
        set => SetValue(ref _focused, value);
    }

    public T Disabled {
        get => _disabled;
        set => SetValue(ref _disabled, value);
    }

    public T All {
        set => SetValues(value);
    }

    public StateValue() { }

    public StateValue(UIComponent parent) => Parent = parent;

    protected StateValue(UIComponent parent, T normal, T hovered, T pressed, T focused, T disabled) {
        Parent = parent;
        _normal = normal;
        _hovered = hovered;
        _pressed = pressed;
        _focused = focused;
        _disabled = disabled;
    }

    protected abstract bool AreEqual(T a, T b);

    public virtual void PerformAutoRender() {
        if (IsPerformingAutoRender) return;

        IsPerformingAutoRender = true;
        Parent?.Invalidate();
        IsPerformingAutoRender = false;
    }

    public virtual void SuspendAutoRender() => _renderSuspendCount++;

    public virtual void ResumeAutoRender(bool performNow = true) {
        if (_renderSuspendCount == 0) return;

        _renderSuspendCount--;

        if (_renderSuspendCount == 0 && performNow)
            PerformAutoRender();
    }

    public T GetValue(InteractionState state) => state switch {
        InteractionState.Hovered => _hovered,
        InteractionState.Pressed => _pressed,
        InteractionState.Focused => _focused,
        InteractionState.Disabled => _disabled,
        _ => _normal
    };

    public void SetValues(T normal, T hovered, T pressed, T focused, T disabled) {
        SuspendAutoRender();
        _normal = normal;
        _hovered = hovered;
        _pressed = pressed;
        _focused = focused;
        _disabled = disabled;
        ResumeAutoRender();
    }

    public void SetValues(T value) {
        SuspendAutoRender();
        _normal = value;
        _hovered = value;
        _pressed = value;
        _focused = value;
        _disabled = value;
        ResumeAutoRender();
    }

    protected void SetValue(ref T field, T value) {
        if (AreEqual(field, value)) return;

        field = value;
        InvalidateRender();
    }

    protected void InvalidateRender() {
        if (IsAutoRenderSuspended) return;

        PerformAutoRender();
    }
}