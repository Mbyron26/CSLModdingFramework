using System;
using System.Collections.Generic;
using ColossalFramework.UI;

namespace CSLModdingFramework.UI.Controls;

public class SegmentedControl<T> : SingleSelectGroupBase<T, SelectableOption<T>> {
    public event Action<SegmentItem> SegmentItemCreated;

    public SegmentList BoundSegmentList { get; private set; }
    public Func<T, IAtlas> DefaultIconAtlasSelector { get; set; }
    public Func<T, string> DefaultIconNameSelector { get; set; }
    public Func<T, string> DefaultTextSelector { get; set; }

    protected override SelectableOption<T> CreateOption(int id, T value) {
        var option = new SelectableOption<T>(id, value) {
            Text = DefaultTextSelector?.Invoke(value) ?? (value != null ? value.ToString() : null),
            IconName = DefaultIconNameSelector?.Invoke(value),
            IconAtlas = DefaultIconAtlasSelector?.Invoke(value),
            IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? true,
            IsVisible = DefaultVisibleSelector?.Invoke(value) ?? true,
        };
        return option;
    }

    protected override void InitializeOption(SelectableOption<T> option) {
        var value = option.Value;
        option.Text ??= DefaultTextSelector?.Invoke(value) ?? (value != null ? value.ToString() : null);
        option.IconName = DefaultIconNameSelector?.Invoke(value);
        option.IconAtlas = DefaultIconAtlasSelector?.Invoke(value);
        option.IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? option.IsEnabled;
        option.IsVisible = DefaultVisibleSelector?.Invoke(value) ?? option.IsVisible;
    }

    protected override void OnOptionAdded(SelectableOption<T> option) => AddSegmentItem(option);

    protected override void OnOptionEnabledChanged(SelectableOption<T> option) {
        if (BoundSegmentList != null && BoundSegmentList.TryGetItem(option.Id, out var item)) {
            item.isEnabled = option.IsEnabled;
        }
    }

    protected override void OnOptionRemoved(SelectableOption<T> option) {
        if (BoundSegmentList == null) return;

        BoundSegmentList.DestroyItem(option.Id);
    }

    protected override void OnOptionSelectionChanged(SelectableOption<T> option, bool notify = true) {
        if (BoundSegmentList != null) {
            BoundSegmentList.TryGetItem(option.Id, out var radioButton);
            radioButton?.SetSelectedWithoutNotify(option.IsSelected);
        }

        base.OnOptionSelectionChanged(option, notify);
    }

    protected override void OnOptionVisibleChanged(SelectableOption<T> option) {
        if (BoundSegmentList != null && BoundSegmentList.TryGetItem(option.Id, out var item)) {
            item.isVisible = option.IsVisible;
        }
    }

    protected override void OnOptionsAdded(List<SelectableOption<T>> options) {
        if (BoundSegmentList == null) return;

        BoundSegmentList.SuspendAutoLayout();

        foreach (var option in options) {
            AddSegmentItem(option);
        }

        BoundSegmentList.ResumeAutoLayout();
    }

    protected override void OnOptionsEnabledChanged(List<SelectableOption<T>> options) {
        if (BoundSegmentList == null) return;

        foreach (var option in options) {
            if (BoundSegmentList.TryGetItem(option.Id, out var radioButton)) {
                radioButton.isEnabled = option.IsEnabled;
            }
        }
    }

    protected override void OnOptionsRemoved(List<SelectableOption<T>> removedOptions) {
        if (BoundSegmentList == null) return;

        BoundSegmentList.SuspendAutoLayout();

        foreach (var option in removedOptions) {
            BoundSegmentList.DestroyItem(option.Id);
        }

        BoundSegmentList.ResumeAutoLayout();
    }

    protected override void OnOptionsSorted() {
        OnOptionsRemoved(_options);
        OnOptionsAdded(_options);
    }

    protected override void OnOptionsVisibleChanged(List<SelectableOption<T>> changedOptions) {
        if (BoundSegmentList == null) return;

        BoundSegmentList.SuspendAutoLayout();
        foreach (var option in changedOptions) {
            if (BoundSegmentList.TryGetItem(option.Id, out var checkbox)) {
                checkbox.isVisible = option.IsVisible;
            }
        }

        BoundSegmentList.ResumeAutoLayout();
    }

    public SelectableOption<T> AddOption(T value, string text = null, string iconName = null, IAtlas iconAtlas = null, bool isEnabled = true, bool isVisible = true) {
        var id = _nextId++;
        var option = new SelectableOption<T>(id, value) {
            Text = text ?? DefaultTextSelector?.Invoke(value) ?? (value != null ? value.ToString() : null),
            IconName = iconName ?? DefaultIconNameSelector?.Invoke(value),
            IsEnabled = DefaultEnabledSelector?.Invoke(value) ?? isEnabled,
            IsVisible = DefaultVisibleSelector?.Invoke(value) ?? isVisible,
        };

        _options.Add(option);
        _optionsById.Add(id, option);
        OnOptionAdded(option);
        return option;
    }

    public void Bind(SegmentList segmentList) {
        if (BoundSegmentList != null || segmentList == null) return;

        BoundSegmentList = segmentList;
        BoundSegmentList.objectUserData = this;
        BoundSegmentList.ItemCreated += OnItemCreated;
        BoundSegmentList.ItemDestroying += OnItemDestroying;

        RebuildSegmentList();
    }

    public void Unbind() {
        if (BoundSegmentList == null) return;

        foreach (var item in BoundSegmentList.Items) {
            item.eventClicked -= OnItemClicked;
        }

        BoundSegmentList.ItemCreated -= OnItemCreated;
        BoundSegmentList.ItemDestroying -= OnItemDestroying;
        BoundSegmentList.objectUserData = null;
        BoundSegmentList = null;
    }

    protected void AddSegmentItem(SelectableOption<T> option) {
        if (BoundSegmentList == null) return;

        var item = BoundSegmentList.AddItem(option.Id);
        SegmentItemCreated?.Invoke(item);
        item.objectUserData = option;
        item.Text = option.Text;
        item.IconSpriteName = option.IconName;
        item.IconAtlas = option.IconAtlas;
        item.isVisible = option.IsVisible;
        item.isEnabled = option.IsEnabled;
        item.SetSelectedWithoutNotify(option.IsSelected);
    }

    private void OnItemClicked(UIComponent component, UIMouseEventParameter eventParam) {
        if (component.objectUserData is not SelectableOption<T> option) return;

        SetSelected(option);
    }

    private void OnItemCreated(SegmentItem sender) => sender.eventClick += OnItemClicked;

    private void OnItemDestroying(SegmentItem sender) => sender.eventClick -= OnItemClicked;

    private void RebuildSegmentList() {
        if (BoundSegmentList == null) return;

        BoundSegmentList.SuspendAutoLayout();
        BoundSegmentList.DestroyItems();
        foreach (var option in _options) {
            AddSegmentItem(option);
        }

        BoundSegmentList.ResumeAutoLayout();
    }
}