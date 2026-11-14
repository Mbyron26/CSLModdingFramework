using System.Collections.Generic;
using System.IO;
using CSLModdingFramework.Configuration;
using CSLModdingFramework.Core;
using CSLModdingFramework.Utilities;

namespace CSLModdingFramework.Serialization;

public sealed class SerializationSystem : SystemBase {
    private UpdateSystem _updateSystem;
    private NewtonsoftReaderAdapter _newtonsoftReaderAdapter;
    private NewtonsoftWriterAdapter _newtonsoftWriterAdapter;
    
    public string SerializationPath { get; private set; }

    private NewtonsoftReaderAdapter Reader => _newtonsoftReaderAdapter ??= new NewtonsoftReaderAdapter(SerializationPath);
    private NewtonsoftWriterAdapter Writer => _newtonsoftWriterAdapter ??= new NewtonsoftWriterAdapter(SerializationPath);

    protected override void OnCreate() {
        base.OnCreate();

        SerializationPath = Path.Combine(SettingsSystem.ModsSettingsDirectory, Domain.CurrentAssemblyName.Name);
        _updateSystem = Domain.GetOrCreateSystem<UpdateSystem>();
        SerializableDataHook.SerializeData += SerializeData;
        SerializableDataHook.DeserializeData += DeserializeData;
    }

    private void SerializeData() {
        EnsureSerializationDirectoryExists();
        var serializationGroup = _updateSystem._serializationGroup;
        SerializeInternal(serializationGroup.Before);
        SerializeInternal(serializationGroup.At);
        SerializeInternal(serializationGroup.After);
    }

    private void DeserializeData() {
        EnsureSerializationDirectoryExists();
        var serializationGroup = _updateSystem._serializationGroup;
        DeserializeInternal(serializationGroup.Before);
        DeserializeInternal(serializationGroup.At);
        DeserializeInternal(serializationGroup.After);
    }

    private void SerializeInternal(List<ISerializable> list) {
        for (var i = 0; i < list.Count; i++) {
            list[i].Serialize(Writer);
        }
    }

    private void DeserializeInternal(List<ISerializable> list) {
        for (var i = 0; i < list.Count; i++) {
            list[i].Deserialize(Reader);
        }
    }

    private void EnsureSerializationDirectoryExists() => DirectoryHelper.EnsureDirectoryExists(SerializationPath);
}