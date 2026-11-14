using ColossalFramework.UI;
using CSLModdingFramework.Lib.Numeric;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class UIInteractiveControl : UIControl {
    protected InteractionState _state = InteractionState.Normal;

    public virtual bool BlurOnClickOutside { get; set; }
    public bool InvalidateOnStateChanged { get; set; } = true;

    public event UIEventHandler<UIInteractiveControl, ValueChangedArgs<InteractionState>> StateChanged;

    public virtual InteractionState State {
        get => _state;
        set => SetInteractionState(value);
    }

    public virtual bool SetInteractionStateWithoutNotify(InteractionState newState) => SetInteractionState(newState, false);

    public virtual bool SetInteractionState(InteractionState newState, bool notify = true) {
        if (!isEnabled)
            newState = InteractionState.Disabled;

        if (_state == newState)
            return false;

        var oldState = _state;
        _state = newState;

        if (notify)
            OnStateChanged(oldState, newState);

        if (InvalidateOnStateChanged)
            Invalidate();

        return true;
    }

    protected virtual void OnStateChanged(InteractionState oldState, InteractionState newState) => StateChanged?.Invoke(this, new ValueChangedArgs<InteractionState> {
        OldValue = oldState,
        NewValue = newState
    });

    protected virtual void CheckForLostFocus() {
        if (!BlurOnClickOutside) return;

        if (!isVisible || !Input.GetMouseButtonDown(0)) return;

        var camera = GetCamera();
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Raycast(ray)) return;

        Unfocus();
    }

    protected virtual void UpdateInteractionStateOnIsEnabledChanged() => State = isEnabled ? InteractionState.Normal : InteractionState.Disabled;

    protected virtual void UpdateInteractionStateOnEnterFocus(UIFocusEventParameter p) {
        if (_state != InteractionState.Pressed)
            State = InteractionState.Focused;
    }

    protected virtual void UpdateInteractionStateOnLeaveFocus(UIFocusEventParameter p) {
        if (_state != InteractionState.Pressed)
            State = containsMouse ? InteractionState.Hovered : InteractionState.Normal;
    }

    protected virtual void UpdateInteractionStateOnMouseDown(UIMouseEventParameter p) {
        if (!canFocus)
            State = InteractionState.Pressed;
    }

    protected virtual void UpdateInteractionStateOnMouseUp(UIMouseEventParameter p) {
        if (containsFocus) {
            State = InteractionState.Focused;
        }
        else if (containsMouse) {
            State = InteractionState.Hovered;
        }
        else {
            State = InteractionState.Normal;
        }
    }

    protected virtual void UpdateInteractionStateOnMouseEnter(UIMouseEventParameter p) {
        if (containsFocus) return;

        if (_state != InteractionState.Pressed)
            State = InteractionState.Hovered;
    }

    protected virtual void UpdateInteractionStateOnMouseLeave(UIMouseEventParameter p) {
        if (_state != InteractionState.Pressed) {
            State = containsFocus
                ? InteractionState.Focused
                : InteractionState.Normal;
        }
    }

    public override void Update() {
        base.Update();

        CheckForLostFocus();
    }

    protected override void OnIsEnabledChanged() {
        UpdateInteractionStateOnIsEnabledChanged();

        base.OnIsEnabledChanged();
    }

    protected override void OnEnterFocus(UIFocusEventParameter p) {
        UpdateInteractionStateOnEnterFocus(p);

        base.OnEnterFocus(p);
    }

    protected override void OnLeaveFocus(UIFocusEventParameter p) {
        UpdateInteractionStateOnLeaveFocus(p);

        base.OnLeaveFocus(p);
    }

    protected override void OnMouseDown(UIMouseEventParameter p) {
        UpdateInteractionStateOnMouseDown(p);

        base.OnMouseDown(p);
    }

    protected override void OnMouseUp(UIMouseEventParameter p) {
        UpdateInteractionStateOnMouseUp(p);

        base.OnMouseUp(p);
    }

    protected override void OnMouseEnter(UIMouseEventParameter p) {
        UpdateInteractionStateOnMouseEnter(p);

        base.OnMouseEnter(p);
    }

    protected override void OnMouseLeave(UIMouseEventParameter p) {
        UpdateInteractionStateOnMouseLeave(p);

        base.OnMouseLeave(p);
    }
}