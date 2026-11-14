using UnityEngine;

namespace CSLModdingFramework.Core;

public sealed class KeyBindingDispatcher : MonoBehaviour {
    private KeyBindingSystem _system;
    
    public void Inject(KeyBindingSystem system) => _system = system;

    private void Update() => _system?.Update();
}