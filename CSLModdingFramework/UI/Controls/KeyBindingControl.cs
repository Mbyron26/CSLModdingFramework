using System;
using ColossalFramework.UI;
using CSLModdingFramework.Core;
using CSLModdingFramework.Localization;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public sealed class KeyBindingControl : UIInteractiveControl, IAutoLayout {
    private Button _inputButton;
    private Icon _resetIcon;
    private Icon _clearIcon;
    private KeyBinding _binding;
    private BindingPhase _bindingPhase = BindingPhase.Idle;
    private string _originalText;
    private int _spacing = 6;
    private int _autoLayoutSuspendCount;

    public event Action<KeyBinding> BindingChanged;

    public KeyBindingControl() => InvalidateOnStateChanged = false;

    public ILocalizationProvider LocalizationProvider { get; set; }
    public bool IsPerformingAutoLayout { get; private set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public KeyBinding Binding {
        get => _binding;
        set => SetBinding(value);
    }

    public int Spacing {
        get => _spacing;
        set {
            if (value.Equals(_spacing)) return;

            _spacing = value;
            InvalidateAutoLayout();
        }
    }

    public override void Awake() {
        base.Awake();
        m_Size = new Vector2(280, 30);

        _inputButton = AddUIComponent<Button>();
        _inputButton.canFocus = true;
        _inputButton.ApplySettingItemControlStyle(ThemeKind.ClassicDark);
        _inputButton.size = new Vector2(280, 30);
        _inputButton.eventMouseDown += OnBindingMouseDown;
        _inputButton.eventKeyDown += OnBindingKeyDown;

        _resetIcon = AddUIComponent<Icon>();
        ApplyIconStyle(_resetIcon);
        _resetIcon.size = new Vector2(30, 30);
        _resetIcon.Foreground.Atlas = DefaultAtlas.Instance;
        _resetIcon.Foreground.Sprites.SetValues(DefaultAtlas.ResetOutline);
        _resetIcon.Foreground.CustomSize = _resetIcon.size;
        _resetIcon.Foreground.ScaleMode = SpriteScaleMode.FixedSize;
        _resetIcon.Foreground.Scale = 0.6f;
        _resetIcon.tooltip = LocalizationProvider is not null ? LocalizationProvider.GetString(LocalizationStrings.Reset) : LocalizationStrings.Reset;
        _resetIcon.eventClicked += OnResetButtonClicked;

        _clearIcon = AddUIComponent<Icon>();
        ApplyIconStyle(_clearIcon);
        _clearIcon.size = _resetIcon.size;
        _clearIcon.Foreground.Atlas = DefaultAtlas.Instance;
        _clearIcon.Foreground.Sprites.SetValues(DefaultAtlas.XClose);
        _clearIcon.Foreground.CustomSize = _clearIcon.size;
        _clearIcon.Foreground.ScaleMode = SpriteScaleMode.FixedSize;
        _clearIcon.Foreground.Scale = 0.6f;
        _clearIcon.tooltip = LocalizationProvider is not null ? LocalizationProvider.GetString(LocalizationStrings.Clear) : LocalizationStrings.Clear;
        _clearIcon.eventClicked += OnClearButtonClicked;

        InvalidateAutoLayout();
    }

    public void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;
        _inputButton.relativePosition = Vector3.zero;
        _resetIcon.relativePosition = new Vector3(_inputButton.relativePosition.x + _inputButton.size.x + _spacing, 0);
        _clearIcon.relativePosition = new Vector3(_resetIcon.relativePosition.x + _resetIcon.size.x + _spacing, 0);
        m_Size.x = _clearIcon.relativePosition.x + _clearIcon.width;
        IsPerformingAutoLayout = false;
    }

    public void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    public void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    private void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    public void SetBindingWithoutNotify(KeyBinding binding) => SetBinding(binding, false);

    public void SetBinding(KeyBinding binding, bool notify = true) {
        if (binding is null || binding.Equals(_binding)) return;

        _binding = binding;
        if (_inputButton != null) {
            _inputButton.Text = binding.ToString();
        }

        if (notify)
            BindingChanged?.Invoke(_binding);
    }

    private void OnResetButtonClicked(UIComponent component, UIMouseEventParameter eventParam) {
        if (_binding is null) return;

        _binding.Reset();
        if (_inputButton != null) {
            _inputButton.Text = _binding.ToString();
        }

        BindingChanged?.Invoke(_binding);
    }

    private void OnClearButtonClicked(UIComponent component, UIMouseEventParameter eventParam) => ApplyKey(KeyCombination.Unbound);

    private void OnBindingMouseDown(UIComponent component, UIMouseEventParameter p) {
        if (_bindingPhase != BindingPhase.Idle) return;

        p.Use();
        _originalText = _inputButton.Text;
        _bindingPhase = BindingPhase.WaitingForKey;
        _inputButton.Text = LocalizationProvider is not null ? LocalizationProvider.GetString(LocalizationStrings.PressAnyKey) : "Press any key";
        _inputButton.Focus();
        UIView.PushModal(_inputButton);

        if (p.buttons != UIMouseButton.Left && p.buttons != UIMouseButton.Right) HandleMouseBinding(p.buttons);
    }

    private void HandleMouseBinding(UIMouseButton button) {
        if (_bindingPhase != BindingPhase.WaitingForKey) return;

        var keyCode = button switch {
            UIMouseButton.Middle => KeyCode.Mouse2,
            UIMouseButton.Special0 => KeyCode.Mouse3,
            UIMouseButton.Special1 => KeyCode.Mouse4,
            UIMouseButton.Special2 => KeyCode.Mouse5,
            UIMouseButton.Special3 => KeyCode.Mouse6,
            _ => KeyCode.None
        };

        if (keyCode != KeyCode.None)
            ApplyKey(new KeyCombination(keyCode, ModifierFlagsExtensions.IsControlDown(), ModifierFlagsExtensions.IsShiftDown(), ModifierFlagsExtensions.IsAltDown()));
    }

    private void OnBindingKeyDown(UIComponent component, UIKeyEventParameter e) {
        if (_bindingPhase == BindingPhase.Idle) return;

        if (e.keycode == KeyCode.Escape) {
            CancelBinding();
            e.Use();
            return;
        }

        if (e.keycode == KeyCode.Backspace) {
            ApplyKey(KeyCombination.Unbound);
            EndBinding();
            e.Use();
            return;
        }

        if (ModifierFlagsExtensions.IsModifierKey(e.keycode)) {
            _bindingPhase = BindingPhase.PreviewingModifiers;
            e.Use();
            return;
        }

        var combination = new KeyCombination(e.keycode, ModifierFlagsExtensions.IsControlDown(), ModifierFlagsExtensions.IsShiftDown(), ModifierFlagsExtensions.IsAltDown());
        ApplyKey(combination);
        EndBinding();
        e.Use();
    }

    private void ApplyKey(KeyCombination keyCombination) {
        if (_binding is null) return;

        _binding.Combination = keyCombination;
        if (_inputButton != null) {
            _inputButton.Text = _binding.ToString();
        }

        BindingChanged?.Invoke(_binding);
    }

    public override void Update() {
        base.Update();

        if (_bindingPhase != BindingPhase.PreviewingModifiers) return;

        var preview = ModifierFlagsExtensions.GetActiveModifierDisplay();
        _inputButton.Text = string.IsNullOrEmpty(preview) ? LocalizationProvider.GetString(LocalizationStrings.PressAnyKey) : preview;
    }

    private void CancelBinding() {
        _inputButton.Text = _originalText;
        _bindingPhase = BindingPhase.Idle;
        UIView.PopModal();
    }

    private void EndBinding() {
        _bindingPhase = BindingPhase.Idle;
        UIView.PopModal();
    }

    private void ApplyIconStyle(Icon icon) {
        icon.Background.Atlas = DefaultAtlas.Instance;
        icon.Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
        icon.Background.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
        icon.Foreground.Atlas = DefaultAtlas.Instance;
    }

    private enum BindingPhase {
        Idle,
        WaitingForKey,
        PreviewingModifiers
    }
}