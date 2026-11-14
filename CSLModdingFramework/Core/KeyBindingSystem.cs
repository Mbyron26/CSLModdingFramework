using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CSLModdingFramework.Core;

public class KeyBindingSystem : SystemBase {
    private readonly List<KeyBindingEntry> _entries = new();
    private readonly List<KeyBindingEntry> _activeEntries = new();
    private bool _activeBindingsDirty;
    private KeyBindingDispatcher _dispatcher;
    private KeyBindingContext _currentContext;

    public bool Enabled { get; set; } = true;

    private KeyBindingContext CurrentContext => CurrentMode switch {
        GameMode.MainMenu => KeyBindingContext.MainMenu,
        GameMode.Game => KeyBindingContext.Game,
        GameMode.MapEditor => KeyBindingContext.MapEditor,
        GameMode.AssetEditor => KeyBindingContext.AssetEditor,
        GameMode.ThemeEditor => KeyBindingContext.ThemeEditor,
        GameMode.ScenarioEditor => KeyBindingContext.ScenarioEditor,
        _ => KeyBindingContext.None
    };

    protected override void OnCreate() {
        base.OnCreate();
        
        if (UIView.GetAView() != null) {
            UpdateContext();
        }
        else {
            LoadingManager.instance.m_introLoaded += UpdateContext;
        }
    }

    protected override void OnGameLoaded(LoadContext context) => UpdateContext();

    protected override void OnGameUnloaded() => UpdateContext();

    public IEnumerable<KeyBindingEntry> GetBindings() => _entries;

    public IEnumerable<KeyBindingEntry> GetActiveBindings() => _activeEntries;

    public bool Register(string id, KeyBinding binding, Action action, KeyBindingContext context = KeyBindingContext.Global, bool overwrite = false) {
        var index = _entries.FindIndex(e => e.Id == id);

        if (index >= 0) {
            if (!overwrite) return false;

            _entries.RemoveAt(index);
        }

        _entries.Add(new KeyBindingEntry(id, binding, action, context));
        _activeBindingsDirty = true;
        UpdateDispatcherState();
        return true;
    }

    public bool Unregister(string id) {
        var removed = _entries.RemoveAll(e => e.Id == id) > 0;

        if (!removed) return false;

        _activeBindingsDirty = true;
        UpdateDispatcherState();
        return true;
    }

    internal void Update() {
        if (!Enabled) return;

        if (_activeBindingsDirty) {
            RefreshActiveBindings();
            _activeBindingsDirty = false;
        }

        if (_activeEntries.Count == 0) return;

        for (var i = 0; i < _activeEntries.Count; i++) {
            var entry = _activeEntries[i];

            if (entry.Binding.IsTriggered())
                entry.Action?.Invoke();
        }
    }

    private void RefreshActiveBindings() {
        _currentContext = CurrentContext;
        _activeEntries.Clear();

        foreach (var entry in _entries) {
            if ((entry.Context & _currentContext) != 0)
                _activeEntries.Add(entry);
        }
    }

    private void UpdateDispatcherState() {
        if (_entries.Count > 0) {
            EnsureDispatcher();
        }
        else {
            DestroyDispatcher();
        }
    }

    private void EnsureDispatcher() {
        if (_dispatcher != null) return;

        var go = new GameObject($"{AssemblyNameString}.KeyBindingDispatcher");
        _dispatcher = go.AddComponent<KeyBindingDispatcher>();
        _dispatcher.Inject(this);

        Object.DontDestroyOnLoad(go);
    }

    private void DestroyDispatcher() {
        if (_dispatcher == null) return;

        Object.Destroy(_dispatcher.gameObject);
        _dispatcher = null;
    }

    private void UpdateContext() {
        var context = CurrentContext;

        if (context == _currentContext) return;

        _currentContext = context;
        _activeBindingsDirty = true;
    }
}