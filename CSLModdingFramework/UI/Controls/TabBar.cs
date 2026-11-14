using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using CSLModdingFramework.Collections;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class TabBar : UIInteractiveControl, IAutoLayout, IThemeable {
    protected readonly List<TabBarItem> _items = new();
    protected readonly Dictionary<string, UIComponent> _pages = new();
    protected readonly List<TabBarItem> _visibleItems = new();
    protected EdgeInsets _padding = new(4);
    protected float _columnGap = 4f;
    protected float _rowGap = 4f;
    protected Orientation _layoutOrientation;
    protected BackgroundRenderer _backgroundRenderer;
    protected IReadOnlyList<TabBarItem> _readOnlyItems;
    protected int _selectedIndex = -1;
    protected int _autoLayoutSuspendCount;
    protected ThemeKind _theme;

    public event UIEventHandler<TabBar, TabBarItem> SelectionChanged;
    public event UIEventHandler<TabBarItem> TabBarItemSetup;

    public TabBar() => InvalidateOnStateChanged = false;

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public bool IsPerformingAutoLayout { get; protected set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;
    public BackgroundRenderer BackgroundRenderer => _backgroundRenderer ??= new BackgroundRenderer(this);
    public IReadOnlyList<TabBarItem> Items => _readOnlyItems ??= _items.AsReadOnlyList();

    public string SelectedId => _selectedIndex >= 0 && _selectedIndex < _items.Count
        ? _items[_selectedIndex].stringUserData
        : null;

    public int SelectedIndex {
        get => _selectedIndex;
        set => SetSelection(value);
    }

    public EdgeInsets Padding {
        get => _padding;
        set {
            if (value.Equals(_padding)) return;

            _padding = value;
            InvalidateAutoLayout();
        }
    }

    public float ColumnGap {
        get => _columnGap;
        set {
            if (Mathf.Approximately(value, _columnGap)) return;

            _columnGap = value;
            InvalidateAutoLayout();
        }
    }

    public Orientation LayoutDirection {
        get => _layoutOrientation;
        set {
            if (value.Equals(_layoutOrientation)) return;

            _layoutOrientation = value;
            InvalidateAutoLayout();
        }
    }

    public float RowGap {
        get => _rowGap;
        set {
            if (Mathf.Approximately(value, _rowGap)) return;

            _rowGap = value;
            InvalidateAutoLayout();
        }
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        if (_items.Count == 0) return;

        IsPerformingAutoLayout = true;
        _visibleItems.Clear();

        foreach (var item in _items) {
            if (item.isVisibleSelf && item.isEnabled && item.name != ExcludedControl)
                _visibleItems.Add(item);
        }

        var count = _visibleItems.Count;
        if (count == 0) {
            IsPerformingAutoLayout = false;
            return;
        }

        switch (_layoutOrientation) {
            case Orientation.Horizontal:
                LayoutHorizontal(_visibleItems);
                break;
            case Orientation.Vertical:
            default:
                LayoutVertical(_visibleItems);
                break;
        }

        IsPerformingAutoLayout = false;
    }

    public virtual void SetTheme(ThemeKind style) {
        if (_theme == style) return;

        _theme = style;
        var backgroundRenderer = BackgroundRenderer;
        backgroundRenderer.Atlas = DefaultAtlas.Instance;
        backgroundRenderer.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);

        backgroundRenderer.Colors.SetValues(style == ThemeKind.ClassicLight ? UIColorPalette.ClassicLightPalette.SettingsSectionBackground : UIColorPalette.ClassicDarkPalette.SettingsSectionBackground);
    }

    public virtual bool SetSelectionWithoutNotify(int index) => SetSelection(index, false);
    public virtual bool SetSelectionWithoutNotify(string id) => SetSelection(id, false);

    public virtual bool SetSelection(int index, bool notify = true) {
        if (index < 0 || index >= _items.Count || _selectedIndex == index) return false;

        if (_selectedIndex >= 0)
            _items[_selectedIndex].IsSelected = false;

        _selectedIndex = index;
        _items[_selectedIndex].IsSelected = true;

        foreach (var kv in _pages)
            kv.Value.isVisible = kv.Key == _items[index].stringUserData;

        if (notify)
            SelectionChanged?.Invoke(this, _items[_selectedIndex]);

        return true;
    }

    public virtual bool SetSelection(string id, bool notify = true) {
        if (string.IsNullOrEmpty(id) || !_pages.TryGetValue(id, out _)) return false;

        var index = _items.FindIndex(i => i.stringUserData == id);
        return SetSelection(index, notify);
    }

    public virtual void SetTabItemVisible(string id, bool visible) {
        if (string.IsNullOrEmpty(id)) return;
        if (_pages.TryGetValue(id, out var page))
            page.isVisible = visible;
        var index = _items.FindIndex(i => i.stringUserData == id);
        if (index != -1 && index < _items.Count)
            _items[index].isVisible = visible;

        PerformAutoLayout();
    }

    public virtual void SetTabItemEnabled(string id, bool enable) {
        if (string.IsNullOrEmpty(id)) return;

        if (_pages.TryGetValue(id, out var page))
            page.isEnabled = enable;

        var index = _items.FindIndex(i => i.stringUserData == id);
        if (index != -1 && index < _items.Count)
            _items[index].enabled = enable;
    }

    public virtual bool AddTab(string id, string text, UIComponent page, Action<TabBarItem> setupTabBarItem = null) {
        if (page == null || string.IsNullOrEmpty(id) || _pages.ContainsKey(id)) return false;

        var item = AddUIComponent<TabBarItem>();
        item.Text = text;
        item.TextPadding = new EdgeInsets(4, 4, 0, 0);
        item.stringUserData = id;
        item.Index = _items.Count;
        item.eventClick += OnTabBarItemClick;
        TabBarItemSetup?.Invoke(item);
        setupTabBarItem?.Invoke(item);

        _pages[id] = page;
        page.stringUserData = id;
        page.isVisible = false;

        _items.Add(item);
        PerformAutoLayout();
        return true;
    }

    protected virtual void OnTabBarItemClick(UIComponent component, UIMouseEventParameter eventParam) {
        if (component is TabBarItem item)
            SetSelection(item.Index);
    }

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    protected override void OnRebuildRenderData() => _backgroundRenderer?.Render();

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended) {
            PerformAutoLayout();
        }

        base.OnSizeChanged();
    }

    private void LayoutHorizontal(IList<TabBarItem> items) {
        var count = items.Count;
        var contentWidth = width - _padding.Horizontal;
        var contentHeight = height - _padding.Vertical;

        var totalGap = _columnGap * (count - 1);
        var buttonWidth = Math.Max(0, (contentWidth - totalGap) / count);

        var startX = _padding.Left;
        var y = _padding.Top;

        for (var i = 0; i < count; i++) {
            var btn = items[i];
            btn.size = new Vector2(buttonWidth, contentHeight);
            btn.relativePosition = new Vector3(startX, y);

            startX += buttonWidth;
            if (i < count - 1) startX += _columnGap;
        }
    }

    private void LayoutVertical(IList<TabBarItem> items) {
        var count = items.Count;
        var contentWidth = width - _padding.Horizontal;
        var contentHeight = height - _padding.Vertical;

        var totalGap = _rowGap * (count - 1);
        var buttonHeight = Math.Max(0, (contentHeight - totalGap) / count);

        var x = _padding.Left;
        var startY = _padding.Top;

        for (var i = 0; i < count; i++) {
            var btn = items[i];
            btn.size = new Vector2(contentWidth, buttonHeight);
            btn.relativePosition = new Vector3(x, startY);

            startY += buttonHeight;
            if (i < count - 1) startY += _rowGap;
        }
    }
}