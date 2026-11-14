using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Core;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Localization;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class SettingsSection : SettingsControl, IThemeable {
    protected readonly List<SettingItem> _items = new();
    protected IAtlas _backgroundAtlas;
    protected Color32 _backgroundColor = UIColorPalette.White;
    protected string _backgroundSpriteName = string.Empty;
    protected float _backgroundTopPadding;
    protected float _gap = 6f;
    protected float _headerHeight;
    protected EdgeInsets _itemLayoutPadding = new(16, 16, 14, 14);
    protected IReadOnlyList<SettingItem> _readOnlyItems;
    protected ThemeKind _theme;
    protected float _visibleItemsHeight;

    public IAtlas BackgroundAtlas {
        get => _backgroundAtlas;
        set {
            if (_backgroundAtlas == value) return;

            _backgroundAtlas = value;
            Invalidate();
        }
    }

    public Color32 BackgroundColor {
        get => _backgroundColor;
        set {
            if (_backgroundColor.EqualsColor(value)) return;

            _backgroundColor = value;
            Invalidate();
        }
    }

    public string BackgroundSpriteName {
        get => _backgroundSpriteName;
        set {
            if (string.Equals(value, _backgroundSpriteName)) return;

            _backgroundSpriteName = value;
            Invalidate();
        }
    }

    public float Gap {
        get => _gap;
        set {
            if (Mathf.Approximately(_gap, value)) return;

            _gap = value;
            InvalidateAutoLayout();
        }
    }

    public EdgeInsets ItemLayoutPadding {
        get => _itemLayoutPadding;
        set {
            if (value.Equals(_itemLayoutPadding)) return;

            _itemLayoutPadding = value;
            if (_items.Count == 0) return;

            foreach (var settingItem in _items) {
                settingItem.LayoutPadding = value;
            }

            InvalidateAutoLayout();
        }
    }

    public IReadOnlyList<SettingItem> Items => _readOnlyItems ??= _items.AsReadOnlyList();

    public int LayoutCount { get; private set; }
    public ThemeKind StyleVariant { get; set; } = ThemeKind.ClassicDark;

    public ThemeKind Theme {
        get => _theme;
        set {
            if (value.Equals(_theme)) return;

            _theme = value;
            SetTheme(value);
        }
    }

    public int VisibleItemCount { get; private set; }

    public virtual DynamicFontRenderer RentDescriptionRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = true;
        fontRenderer.MaxSize = new Vector2(width - _textHorizontalPadding.Horizontal, 4096);
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _descriptionScale;
        fontRenderer.LayoutOffset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + new Vector3(TextHorizontalPadding.Left, _headerHeight + _gap);
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = true;
        fontRenderer.DefaultColor = isEnabled ? _descriptionNormalColor : _descriptionDisabledColor;
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    public virtual DynamicFontRenderer RentHeaderRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.WordWrap = true;
        fontRenderer.MaxSize = new Vector2(width - TextHorizontalPadding.Horizontal, 4096);
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _headerScale;
        fontRenderer.LayoutOffset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + new Vector3(TextHorizontalPadding.Left, 0);
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = true;
        fontRenderer.DefaultColor = isEnabled ? _headerNormalColor : _headerDisabledColor;
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    public virtual void SetTheme(ThemeKind theme) {
        BackgroundAtlas = DefaultAtlas.Instance;
        BackgroundSpriteName = DefaultAtlas.SmoothRectangleRadius8;
        switch (theme) {
            case ThemeKind.ClassicLight:
                BackgroundColor = UIColorPalette.ClassicLightPalette.SettingsSectionBackground;
                break;
            case ThemeKind.ClassicDark:
                BackgroundColor = UIColorPalette.ClassicDarkPalette.SettingsSectionBackground;
                break;
            case ThemeKind.ModernDark:
            default:
                BackgroundColor = UIColorPalette.ModernDarkPalette.SettingsSectionBackground;
                break;
        }
    }

    protected virtual void RenderBackground() {
        if (_backgroundAtlas == null || VisibleItemCount == 0) return;

        renderData.material = _backgroundAtlas.Material;
        if (!_backgroundAtlas.TryGetSprite(_backgroundSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _backgroundAtlas.GetSprite(spriteIndex);
        SpriteRenderData spriteRenderData = new() {
            Atlas = _backgroundAtlas,
            Color = _backgroundColor,
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = new Vector3(0, _backgroundTopPadding, 0),
            UnitsPerPixel = UnitsPerPixel,
            Size = new Vector2(width, _visibleItemsHeight),
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }

    protected virtual void RenderDescription() {
        if (Font is not { IsValid: true }) return;

        if (_descriptionRenderData != null) {
            _descriptionRenderData.Clear();
        }
        else {
            _descriptionRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_descriptionRenderData);
        }

        if (string.IsNullOrEmpty(_header) || string.IsNullOrEmpty(_description)) return;

        using var fontRenderer = RentDescriptionRenderer();
        fontRenderer.Render(_description, _descriptionRenderData);
    }

    protected virtual void RenderHeader() {
        if (Font is not { IsValid: true }) return;

        if (_headerRenderData != null) {
            _headerRenderData.Clear();
        }
        else {
            _headerRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_headerRenderData);
        }

        if (string.IsNullOrEmpty(_header)) return;

        using var fontRenderer = RentHeaderRenderer();
        fontRenderer.Render(_header, _headerRenderData);
        _headerHeight = fontRenderer.RenderedSize.y;
    }

    public override void Awake() {
        base.Awake();

        _headerNormalColor = UIColorPalette.White50Percent;
        _descriptionNormalColor = UIColorPalette.White30Percent;
        _textHorizontalPadding = new EdgeInsetsHorizontal(16);
        _headerScale = 0.8f;
        _descriptionScale = 0.8f;
    }

    public override void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        LayoutCount++;

        var count = 0;
        foreach (var item in _items) {
            if (!item.isVisibleSelf) continue;

            count++;
        }

        VisibleItemCount = count;

        if (!string.IsNullOrEmpty(_header)) {
            float headerHeight;
            using (var headerRenderer = RentHeaderRenderer()) {
                headerHeight = headerRenderer.GetRenderedSize(_header).y;
            }

            headerHeight += _gap;

            if (!string.IsNullOrEmpty(_description)) {
                using var descriptionRenderer = RentDescriptionRenderer();
                var descriptionHeight = descriptionRenderer.GetRenderedSize(_description).y;
                headerHeight += descriptionHeight + _gap;
            }

            _backgroundTopPadding = headerHeight;

            if (VisibleItemCount == 0) {
                _visibleItemsHeight = 0;
                SetHeightIfChanged(headerHeight);
            }
            else {
                var currentY = headerHeight;
                var visibleItemsHeight = 0f;
                foreach (var item in _items) {
                    if (!item.isVisibleSelf) continue;

                    item.relativePosition = new Vector3(0, currentY);
                    currentY += item.height;
                    visibleItemsHeight += item.height;
                }

                _visibleItemsHeight = visibleItemsHeight;
                SetHeightIfChanged(currentY);
            }
        }
        else {
            _backgroundTopPadding = 0;
            if (VisibleItemCount == 0) {
                _visibleItemsHeight = 0;
                SetHeightIfChanged(20);
            }
            else {
                var targetHeight = 0f;
                foreach (var item in _items) {
                    if (!item.isVisibleSelf) continue;

                    item.relativePosition = new Vector3(0, targetHeight);
                    targetHeight += item.height;
                }

                _visibleItemsHeight = targetHeight;
                SetHeightIfChanged(_visibleItemsHeight);
            }
        }

        IsPerformingAutoLayout = false;
    }

    protected override void OnRebuildRenderData() {
        RenderHeader();
        RenderDescription();
        RenderBackground();
    }

    #region KeyBinding

    public SettingItem AddKeyBinding(KeyBinding instance, string header, string description = null, Action<KeyBinding> bindingChanged = null, Action<KeyBindingControl> onControlCreated = null) {
        var settingItem = AddSettingItem(header, description);
        var keyBindingControl = settingItem.AddContent<KeyBindingControl>();
        keyBindingControl.Binding = instance;
        keyBindingControl.BindingChanged += bindingChanged;
        onControlCreated?.Invoke(keyBindingControl);
        return settingItem;
    }

    #endregion

    public void DestroyAllItems() {
        if (_items.Count == 0) return;

        SuspendAutoLayout();
        try {
            foreach (var item in _items) {
                item.DestroyChildrenAndSelf();
            }

            _items.Clear();
        }
        finally {
            ResumeAutoLayout();
        }
    }

    public void DestroyItem(SettingItem settingItem) {
        if (settingItem == null) return;

        if (!_items.Remove(settingItem)) return;

        SuspendAutoLayout();
        settingItem.DestroyChildrenAndSelf();
        ResumeAutoLayout();
    }

    #region SpriteView

    public SlicedSpriteView AddSlicedSpriteView(string header, string description = null) {
        var settingItem = AddSettingItem(header, description);
        return settingItem.AddContent<SlicedSpriteView>();
    }

    public SpriteView AddSpriteView(string header, string description = null) {
        var settingItem = AddSettingItem(header, description);
        return settingItem.AddContent<SpriteView>();
    }

    #endregion

    #region TextLabel

    public TextLabel AddTextLabel(string header, string text, string description = null, Orientation direction = Orientation.Horizontal) {
        var settingItem = AddSettingItem(header, description, direction);
        var textLabel = settingItem.AddContent<TextLabel>();
        SetupTextLabel(textLabel, text);
        return textLabel;
    }

    private static void SetupTextLabel(TextLabel textLabel, string text) {
        textLabel.SuspendAutoLayout();
        textLabel.Text = text;
        textLabel.SizeMode = TextSizeMode.FitContent;
        textLabel.TextHorizontalAlignment = UIHorizontalAlignment.Left;
        textLabel.InvalidateOnStateChanged = false;
        textLabel.ResumeAutoLayout();
    }

    #endregion

    #region Fields

    public ByteField AddByteField(string header, string description, byte value, Action<byte> valueChanged = null, UIEventHandler<ValueField, byte> fieldValueChanged = null, float fieldWidth = 200f, float fieldHeight = 30f) => AddNumericField<ByteField, byte>(header, description, value, valueChanged, fieldValueChanged, fieldWidth, fieldHeight);

    public LongField AddLongField(string header, string description, long value, Action<long> valueChanged = null, UIEventHandler<ValueField, long> fieldValueChanged = null, float fieldWidth = 200f, float fieldHeight = 30f) => AddNumericField<LongField, long>(header, description, value, valueChanged, fieldValueChanged, fieldWidth, fieldHeight);

    public FloatField AddFloatField(string header, string description, float value, Action<float> valueChanged = null, UIEventHandler<ValueField, float> fieldValueChanged = null, float fieldWidth = 200f, float fieldHeight = 30f) => AddNumericField<FloatField, float>(header, description, value, valueChanged, fieldValueChanged, fieldWidth, fieldHeight);

    public IntField AddIntField(string header, string description, int value, Action<int> valueChanged = null, UIEventHandler<ValueField, int> fieldValueChanged = null, float fieldWidth = 200f, float fieldHeight = 30f) => AddNumericField<IntField, int>(header, description, value, valueChanged, fieldValueChanged, fieldWidth, fieldHeight);

    public TField AddNumericField<TField, TValue>(string header, string description, TValue value, Action<TValue> valueChanged, UIEventHandler<ValueField, TValue> fieldValueChanged, float fieldWidth, float fieldHeight)
        where TField : NumericField<TValue> where TValue : struct, IComparable<TValue> {
        var settingItem = AddSettingItem(header, description);
        var field = settingItem.AddContent<TField>();

        field.SuspendAutoLayout();
        SetupField(field, fieldWidth, fieldHeight);
        field.Value = value;
        if (fieldValueChanged != null)
            field.FieldValueChanged += fieldValueChanged;

        if (valueChanged != null)
            field.ValueChanged += valueChanged;

        field.ResumeAutoLayout();

        return field;
    }

    public TextField AddReadonlyTextField(string header, string description, string text, Orientation direction = Orientation.Vertical, float fieldWidth = 700f, float fieldHeight = 30f) {
        var textField = AddTextField(header, description, text, direction, fieldWidth, fieldHeight);
        textField.ReadOnly = true;
        return textField;
    }

    public TextField AddTextField(string header, string description, string text, Orientation direction = Orientation.Vertical, float fieldWidth = 700f, float fieldHeight = 30f) {
        var settingItem = AddSettingItem(header, description, direction);
        var textField = settingItem.AddContent<TextField>();
        textField.SuspendAutoLayout();
        SetupField(textField, fieldWidth, fieldHeight);
        textField.Text = text;
        textField.ResumeAutoLayout();
        return textField;
    }

    private static void SetupField(ValueField field, float fieldWidth, float fieldHeight) {
        field.Theme = ThemeKind.ClassicDark;
        field.SetSizeIfChanged(new Vector2(fieldWidth, fieldHeight));
    }

    #endregion

    #region Slider

    public TextSlider AddTextSlider(string header, string description, float defaultValue, float minValue, float maxValue, float stepSize, Action<float> onValueChanged = null, UIEventHandler<TextSlider, float> onSliderValueChanged = null, string text = null, Func<float, string> valueFormatter = null) {
        var settingItem = AddSettingItem(header, description, Orientation.Vertical);

        var slider = settingItem.AddContent<TextSlider>();
        slider.size = new Vector2(settingItem.width - settingItem.LayoutPadding.Horizontal, slider.height);
        slider.Text = text;
        if (valueFormatter != null) {
            slider.ValueFormatter = valueFormatter;
        }

        SetupSlider(slider, minValue, maxValue, defaultValue, stepSize);
        slider.ValueChanged += onValueChanged;
        slider.SliderValueChanged += onSliderValueChanged;
        return slider;
    }

    public Slider AddSlider(string header, string description, float defaultValue, float minValue, float maxValue, float stepSize, Action<float> onValueChanged = null, UIEventHandler<Slider, float> onSliderValueChanged = null) {
        var settingItem = AddSettingItem(header, description, Orientation.Vertical);

        var slider = settingItem.AddContent<Slider>();
        slider.size = new Vector2(settingItem.width - settingItem.LayoutPadding.Horizontal, 16);
        SetupSlider(slider, minValue, maxValue, defaultValue, stepSize);
        slider.ValueChanged += onValueChanged;
        slider.SliderValueChanged += onSliderValueChanged;
        return slider;
    }

    private void SetupSlider(SliderBase slider, float minValue, float maxValue, float defaultValue, float stepSize) {
        slider.MinValue = minValue;
        slider.MaxValue = maxValue;
        slider.StepSize = stepSize;
        slider.SetValueWithoutNotify(defaultValue);
        if (StyleVariant == ThemeKind.ClassicLight) {
            slider.EnableMouseWheel = true;
            slider.EnableArrowKeys = true;
            slider.SetTheme(ThemeKind.ClassicLight);
        }
        else {
            slider.SetTheme(ThemeKind.ClassicDark);
        }
    }

    #endregion

    #region Toggle

    public Toggle AddToggle(string header, string description, bool isOn, Action<bool> onToggleChanged = null) {
        var settingItem = AddSettingItem(header, description);
        var toggle = settingItem.AddContent<Toggle>();
        toggle.SetTheme(StyleVariant);
        toggle.SetToggleStateWithoutNotify(isOn);
        toggle.IsOnChanged += onToggleChanged;
        return toggle;
    }

    public Toggle AddToggle(string header, string description, bool isOn, UIEventHandler<Toggle, bool> onToggleChanged = null) {
        var settingItem = AddSettingItem(header, description);
        var toggle = settingItem.AddContent<Toggle>();
        toggle.SetTheme(StyleVariant);
        toggle.SetToggleStateWithoutNotify(isOn);
        toggle.ToggleChanged += onToggleChanged;
        return toggle;
    }

    #endregion

    #region Button

    public Button AddButton(string header, string description, string text, MouseEventHandler onClick = null) {
        var settingItem = AddSettingItem(header, description);
        var button = settingItem.AddContent<Button>();
        SetupButton(button, text);
        button.eventClicked += onClick;
        return button;
    }

    public Button AddButton(string header, string description, string text, Action onClick = null) {
        var settingItem = AddSettingItem(header, description);
        var button = settingItem.AddContent<Button>();
        SetupButton(button, text);
        button.Clicked += onClick;
        return button;
    }

    private void SetupButton(Button button, string text) {
        button.SuspendAutoLayout();
        button.SizeMode = TextSizeMode.FitWidth;
        button.SetHeightIfChanged(30);
        button.TextHorizontalAlignment = UIHorizontalAlignment.Left;
        button.TextPadding = new EdgeInsets(10, 10, 0, 0);
        button.ApplyStyle(StyleVariant);
        button.Text = text;
        button.ResumeAutoLayout();
    }

    #endregion

    #region CheckBox

    public DropdownCheckboxTriggerButton AddDropdownCheckboxTriggerButton<T>(DropdownCheckboxGroup<T> groupInstance, string header, string description = null) {
        var item = AddSettingItem(header, description, Orientation.Vertical);
        var control = item.AddContent<DropdownCheckboxTriggerButton>();

        groupInstance.Bind(control);
        return control;
    }

    public DropdownCheckboxGroup<T> AddDropdownCheckboxGroup<T>(string header, string description = null) {
        var group = new DropdownCheckboxGroup<T>();
        group.CheckboxCreated += CheckboxSetup;
        group.SelectAllCheckboxCreated += CheckboxSetup;

        var item = AddSettingItem(header, description, Orientation.Vertical);
        var control = item.AddContent<DropdownCheckboxTriggerButton>();

        group.Bind(control);
        return group;
    }

    public ScrollableCheckboxListBox AddScrollableCheckboxListBox<T>(ScrollableCheckboxGroup<T> groupInstance, string header, string description = null, float maxHeight = 200) {
        var settingItem = AddSettingItem(header, description, Orientation.Vertical);

        var listBox = settingItem.AddContent<ScrollableCheckboxListBox>();
        listBox.width = settingItem.width - settingItem.LayoutPadding.Horizontal;
        listBox.List.width = listBox.width - 8 - 4;
        listBox.height = maxHeight;

        groupInstance.Bind(listBox);
        return listBox;
    }

    public ScrollableCheckboxGroup<T> AddScrollableCheckboxGroup<T>(string header, string description = null, float maxHeight = 200) {
        var group = new ScrollableCheckboxGroup<T>();
        group.CheckboxCreated += CheckboxSetup;
        group.SelectAllCheckboxCreated += CheckboxSetup;

        var settingItem = AddSettingItem(header, description, Orientation.Vertical);

        var listBox = settingItem.AddContent<ScrollableCheckboxListBox>();
        listBox.width = settingItem.width - settingItem.LayoutPadding.Horizontal;
        listBox.List.width = listBox.width - 8 - 4;
        listBox.height = maxHeight;

        group.Bind(listBox);

        return group;
    }

    public CheckboxList AddCheckboxList<T>(CheckboxGroup<T> groupInstance, string header, string description = null) {
        if (groupInstance is null)
            throw new ArgumentNullException(nameof(groupInstance));

        var settingItem = AddSettingItem(header, description, Orientation.Vertical);

        var checkboxOptionListBox = settingItem.AddContent<CheckboxList>();
        checkboxOptionListBox.width = settingItem.width - settingItem.LayoutPadding.Horizontal;

        groupInstance.Bind(checkboxOptionListBox);
        return checkboxOptionListBox;
    }

    public CheckboxGroup<T> AddCheckboxGroup<T>(string header = null, string description = null, ILocalizationProvider localizationProvider = null) {
        var checkboxGroup = new CheckboxGroup<T>();
        checkboxGroup.CheckboxCreated += CheckboxSetup;
        checkboxGroup.SelectAllCheckboxCreated += CheckboxSetup;
        if (localizationProvider != null)
            checkboxGroup.LocalizationSystem = localizationProvider;

        var settingItem = AddSettingItem(header, description, Orientation.Vertical);

        var checkboxOptionListBox = settingItem.AddContent<CheckboxList>();
        checkboxOptionListBox.width = settingItem.width - settingItem.LayoutPadding.Horizontal;

        checkboxGroup.Bind(checkboxOptionListBox);
        return checkboxGroup;
    }

    private static void CheckboxSetup(Checkbox checkBox) => checkBox.Theme = ThemeKind.ClassicDark;

    #endregion

    #region RadioGroup

    public RadioButtonList AddRadioButtonList<T>(string header, string description = null, RadioGroup<T> radioGroupInstance = null) {
        var settingItem = AddSettingItem(header, description, Orientation.Vertical);

        var radioOptionListBox = settingItem.AddContent<RadioButtonList>();
        radioOptionListBox.SetWidthIfChanged(settingItem.width - settingItem.LayoutPadding.Horizontal);
        radioOptionListBox.SetHeightIfChanged(20);

        radioGroupInstance?.Bind(radioOptionListBox);
        return radioOptionListBox;
    }

    public RadioGroup<T> AddRadioGroup<T>(string header, string description = null) {
        var settingItem = AddSettingItem(header, description, Orientation.Vertical);

        var radioOptionListBox = settingItem.AddContent<RadioButtonList>();
        radioOptionListBox.SetWidthIfChanged(settingItem.width - settingItem.LayoutPadding.Horizontal);
        radioOptionListBox.SetHeightIfChanged(20);

        var radioButtonGroup = new RadioGroup<T>();
        radioButtonGroup.RadioButtonCreated += OnRadioButtonItemSetup;
        radioButtonGroup.Bind(radioOptionListBox);
        return radioButtonGroup;
    }

    private static void OnRadioButtonItemSetup(RadioButton obj) => obj.SetTheme(ThemeKind.ClassicDark);

    #endregion

    #region DropDown

    public Dropdown<string> AddStringListDropDown(string header, string description, IEnumerable<string> list, string defaultValue, Action<string> onSelectionChanged = null) {
        var group = AddDropdown<string>(header, description);
        group.AddOptions(list);
        group.ValueSelectionChanged += onSelectionChanged;
        group.SetSelectedWithoutNotify(v => string.Equals(v.Value, defaultValue));
        return group;
    }

    public DropdownTriggerButton AddDropdown<T>(string header, string description, Dropdown<T> instance) {
        var settingItem = AddSettingItem(header, description);
        var triggerButton = settingItem.AddContent<DropdownTriggerButton>();
        SetupDropdownTriggerButton(triggerButton);

        if (instance == null) return triggerButton;

        instance.Bind(triggerButton);
        return triggerButton;
    }

    public Dropdown<T> AddDropdown<T>(string header, string description = null) {
        var settingItem = AddSettingItem(header, description);
        var triggerButton = settingItem.AddContent<DropdownTriggerButton>();
        SetupDropdownTriggerButton(triggerButton);

        var dropdown = new Dropdown<T> {
            CanWheel = true
        };
        dropdown.SelectableItemCreated += SetupDropdownItem;
        dropdown.PopupCreated += OnDropdownPopupCreated;
        dropdown.Bind(triggerButton);
        return dropdown;
    }

    private void SetupDropdownTriggerButton(DropdownTriggerButton triggerButton) {
        triggerButton.size = new Vector2(310, 30);
        triggerButton.TextPadding = new EdgeInsets(8, 8, 0, 0);
        triggerButton.SetTheme(ThemeKind.ClassicDark);
        triggerButton.tooltipBox = UIView.GetAView().defaultTooltipBox;
    }

    private void OnDropdownPopupCreated(DropdownTriggerButton sender, SelectableListBox value) {
        value.SizeMode = ScrollViewer.ScrollSizePolicy.FitContent;
        value.MinFitSize = new Size(sender.size.x, sender.size.y);
        value.MaxFitSize = new Size(500, 300);
    }

    private void SetupDropdownItem(SelectableItem obj) {
        obj.size = new Vector2(100, 30);
        obj.TextPadding = new EdgeInsets(4, 4, 0, 0);
        obj.SetTheme(ThemeKind.ClassicDark);
    }

    #endregion

    #region SettingItem

    public SettingItem AddSettingItem(string header = null, string description = null, Orientation direction = Orientation.Horizontal) {
        var item = CreateSettingItem(direction);
        FinalizeItem(item, header, description);
        return item;
    }

    [Obsolete("AddItem is obsolete, use AddSettingItem instead")]
    public SettingItem AddItem<TControl>(string header = null, string description = null, Orientation direction = Orientation.Horizontal, Action<TControl> onControlCreated = null, Action<SettingItem> onItemCreated = null)
        where TControl : UIComponent {
        var item = CreateSettingItem(direction);
        onItemCreated?.Invoke(item);
        var control = item.AddContent<TControl>();
        onControlCreated?.Invoke(control);
        FinalizeItem(item, header, description);
        return item;
    }

    private SettingItem CreateSettingItem(Orientation direction) {
        var item = AddUIComponent<SettingItem>();
        item.eventVisibilityChanged += OnItemVisibilityChanged;
        item.eventSizeChanged += OnItemSizeChanged;

        item.SuspendAutoLayout();
        item.width = width;
        item.Direction = direction;
        item.LayoutPadding = _itemLayoutPadding;

        item.SetTheme(_theme);

        _items.Add(item);
        RenderItemPanelFg();
        return item;
    }

    private void FinalizeItem(SettingItem item, string header, string description) {
        if (!string.IsNullOrEmpty(header))
            item.Header = header;
        if (!string.IsNullOrEmpty(description))
            item.Description = description;

        item.ResumeAutoLayout();
    }

    private void OnItemSizeChanged(UIComponent component, Vector2 value) => InvalidateAutoLayout();
    private void OnItemVisibilityChanged(UIComponent component, bool value) => InvalidateAutoLayout();

    private void RenderItemPanelFg() {
        var count = _items.Count;
        for (var i = 0; i < count; i++) {
            _items[i].BackgroundIsEnabled = i != count - 1;
        }
    }

    #endregion
}