using System;
using ICities;
using UnityEngine;

namespace CSLModdingFramework.Core;

public class SerializableDataHook : ISerializableDataExtension {
    public static Action SerializeData;
    public static Action DeserializeData;

    public void OnCreated(ISerializableData serializableData) => Debug.Log("SerializableDataHook created");

    public void OnLoadData() => DeserializeData?.Invoke();

    public void OnSaveData() => SerializeData?.Invoke();

    public void OnReleased() => Debug.Log("SerializableDataHook released");
}