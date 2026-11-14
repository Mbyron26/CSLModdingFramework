using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Controls;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class RadioGroupBase<T> : RadioOptionsGroupBase<T> {
    public event Action<RadioButton> RadioButtonCreated;

    public abstract RadioButtonList BoundRadioButtonList { get; }

    public InteractionArea InteractionBound { get; set; }

    protected virtual void OnRadioButtonCreated(RadioButton sender) => sender.eventClicked += OnRadioButtonClicked;

    protected virtual void OnRadioDestroying(RadioButton sender) => sender.eventClicked -= OnRadioButtonClicked;

    protected virtual void RebuildRadioButtonList() {
        if (BoundRadioButtonList == null) return;

        BoundRadioButtonList.SuspendAutoLayout();
        BoundRadioButtonList.DestroyItems();
        foreach (var option in _options) {
            AddRadioButton(option);
        }

        BoundRadioButtonList.ResumeAutoLayout();
    }

    protected override void OnOptionAdded(RadioOption<T> option) => AddRadioButton(option);

    protected override void OnOptionEnabledChanged(RadioOption<T> option) {
        if (BoundRadioButtonList != null && BoundRadioButtonList.ItemsById.TryGetValue(option.Id, out var radioButton)) {
            radioButton.isEnabled = option.IsEnabled;
        }
    }

    protected override void OnOptionRemoved(RadioOption<T> option) {
        if (BoundRadioButtonList == null) return;

        BoundRadioButtonList.DestroyItem(option.Id);
    }

    protected override void OnOptionSelectionChanged(RadioOption<T> option, bool notify = true) {
        if (BoundRadioButtonList != null) {
            BoundRadioButtonList.TryGetItem(option.Id, out var radioButton);
            radioButton?.SetSelectionWithoutNotify(option.IsSelected);
        }

        base.OnOptionSelectionChanged(option, notify);
    }

    protected override void OnOptionVisibleChanged(RadioOption<T> option) {
        if (BoundRadioButtonList.ItemsById.TryGetValue(option.Id, out var checkbox)) {
            checkbox.isVisible = option.IsVisible;
        }
    }

    protected override void OnOptionsAdded(List<RadioOption<T>> options) {
        if (BoundRadioButtonList == null) return;

        BoundRadioButtonList.SuspendAutoLayout();

        foreach (var option in options) {
            AddRadioButton(option);
        }

        BoundRadioButtonList.ResumeAutoLayout();
    }

    protected override void OnOptionsEnabledChanged(List<RadioOption<T>> options) {
        if (BoundRadioButtonList == null) return;

        foreach (var option in options) {
            if (BoundRadioButtonList.TryGetItem(option.Id, out var radioButton)) {
                radioButton.isEnabled = option.IsEnabled;
            }
        }
    }

    protected override void OnOptionsRemoved(List<RadioOption<T>> removedOptions) {
        if (BoundRadioButtonList == null) return;

        BoundRadioButtonList.SuspendAutoLayout();

        foreach (var option in removedOptions) {
            BoundRadioButtonList.DestroyItem(option.Id);
        }

        BoundRadioButtonList.ResumeAutoLayout();
    }
    
    protected override void OnOptionsSorted() {
        OnOptionsRemoved(_options);
        OnOptionsAdded(_options);
    }

    protected override void OnOptionsVisibleChanged(List<RadioOption<T>> changedOptions) {
        if (BoundRadioButtonList == null) return;

        BoundRadioButtonList.SuspendAutoLayout();
        foreach (var option in changedOptions) {
            if (BoundRadioButtonList.TryGetItem(option.Id, out var checkbox)) {
                checkbox.isVisible = option.IsVisible;
            }
        }

        BoundRadioButtonList.ResumeAutoLayout();
    }

    protected void AddRadioButton(RadioOption<T> option) {
        if (BoundRadioButtonList == null) return;

        var button = BoundRadioButtonList.AddItem(option.Id);
        RadioButtonCreated?.Invoke(button);
        button.objectUserData = option;
        button.Header = option.Header;
        button.Description = option.Description;
        button.isVisible = option.IsVisible;
        button.isEnabled = option.IsEnabled;
        button.SetSelectionWithoutNotify(option.IsSelected);
    }

    protected void OnRadioButtonClicked(UIComponent component, UIMouseEventParameter eventParam) {
        var radioButton = component as RadioButton;
        if (radioButton == null) return;

        if (!radioButton.GetHitPosition(eventParam.ray, out var hitPosition))
            return;

        switch (InteractionBound) {
            case InteractionArea.Indicator: {
                var hitRect = new Rect(radioButton.LayoutPadding.Left, radioButton.LayoutPadding.Top, radioButton.IndicatorSize.x, radioButton.IndicatorSize.y);

                if (hitRect.Contains(hitPosition)) {
                    RadioButtonClicked(radioButton);
                }

                break;
            }

            case InteractionArea.Content: {
                var width = radioButton.IndicatorSize.x;
                var height = radioButton.IndicatorSize.y;

                var hasHeader = !string.IsNullOrEmpty(radioButton.Header);
                var hasDescription = !string.IsNullOrEmpty(radioButton.Description);

                if (hasHeader) {
                    width = Mathf.Max(width, radioButton.IndicatorSize.x + radioButton.IndicatorTextGap + radioButton.HeaderRenderSize.x);

                    height = Mathf.Max(height, radioButton.HeaderRenderSize.y);

                    if (hasDescription) {
                        width = Mathf.Max(width, radioButton.IndicatorSize.x + radioButton.IndicatorTextGap + radioButton.DescriptionRenderSize.x);

                        height += radioButton.TextGap + radioButton.DescriptionRenderSize.y;
                    }
                }

                var hitRect = new Rect(radioButton.LayoutPadding.Left, radioButton.LayoutPadding.Top, width, height);

                if (hitRect.Contains(hitPosition)) {
                    RadioButtonClicked(radioButton);
                }

                break;
            }

            case InteractionArea.Container:
            default: {
                RadioButtonClicked(radioButton);
                break;
            }
        }
    }

    private void RadioButtonClicked(RadioButton radioButton) {
        if (radioButton.objectUserData is not RadioOption<T> option) return;

        SetSelected(option);
    }
}