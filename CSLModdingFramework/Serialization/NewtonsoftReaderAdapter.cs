using System;
using System.IO;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Logging;
using Newtonsoft.Json.Linq;

namespace CSLModdingFramework.Serialization;

public class NewtonsoftReaderAdapter : IReader {
    private readonly string _baseDir;
    private SerializationContext _context;

    public NewtonsoftReaderAdapter(string baseDir) {
        if (string.IsNullOrEmpty(baseDir))
            throw new ArgumentNullException(nameof(baseDir));

        _baseDir = baseDir;
    }

    public void StartRead(string fileName, string sectionName) {
        if (_context != null)
            throw new InvalidOperationException("A read operation is already in progress");

        var filePath = Path.Combine(_baseDir, $"{fileName}.json");

        JObject root;

        if (!File.Exists(filePath)) {
            root = new JObject();
        }
        else {
            var json = File.ReadAllText(filePath);

            if (json.IsNullOrWhiteSpace()) {
                root = new JObject();
            }
            else {
                try {
                    var token = JToken.Parse(json);
                    if (token is JObject obj)
                        root = obj;
                    else
                        root = new JObject();
                }
                catch (Exception e) {
                    LogManager.GetLogger()
                        .Error(
                            e,
                            $"{nameof(NewtonsoftReaderAdapter)}.{nameof(StartRead)}: " +
                            $"Failed to parse JSON file: {filePath}, json string: {json}\n" +
                            "Starting with an empty object.");

                    root = new JObject();
                }
            }
        }

        var current = root[sectionName] as JObject ?? new JObject();

        _context = new SerializationContext(filePath, sectionName, root, current);
    }

    public void GetProperty<T>(string name, ref T value) {
        if (_context == null)
            throw new InvalidOperationException("Call StartRead first");

        if (!_context.Current.TryGetValue(name, out var token)) return;

        try {
            value = token.ToObject<T>();
        }
        catch (Exception e) {
            LogManager.GetLogger()
                .Error(
                    e,
                    $"{nameof(NewtonsoftReaderAdapter)}.{nameof(GetProperty)}: " +
                    $"Failed to deserialize property '{name}' as {typeof(T).Name}. " +
                    $"File: {_context.FilePath}, Section: {_context.SectionName}.");
        }
    }

    public T GetProperty<T>(string name) {
        if (_context == null)
            throw new InvalidOperationException("Call StartRead first");

        if (!_context.Current.TryGetValue(name, out var token)) return default!;

        try {
            return token.ToObject<T>();
        }
        catch (Exception e) {
            LogManager.GetLogger()
                .Error(
                    e,
                    $"{nameof(NewtonsoftReaderAdapter)}.{nameof(GetProperty)}: " +
                    $"Failed to deserialize property '{name}' as {typeof(T).Name}. " +
                    $"File: {_context.FilePath}, Section: {_context.SectionName}.");

            return default;
        }
    }

    public void EndRead() => _context = null;
}