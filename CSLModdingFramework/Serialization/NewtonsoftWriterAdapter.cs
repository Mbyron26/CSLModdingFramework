using System;
using System.IO;
using System.Linq;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSLModdingFramework.Serialization;

public class NewtonsoftWriterAdapter : IWriter {
    private readonly string _baseDir;
    private SerializationContext _context;
    private SectionWriteMode _sectionWriteMode;

    public NewtonsoftWriterAdapter(string baseDir) {
        if (string.IsNullOrEmpty(baseDir))
            throw new ArgumentNullException(nameof(baseDir));

        _baseDir = baseDir;
    }

    public void EndWrite() {
        if (_context == null)
            throw new InvalidOperationException("Call StartWrite first");

        switch (_sectionWriteMode) {
            case SectionWriteMode.Replace:
                _context.Root[_context.SectionName] = _context.Current;
                break;

            case SectionWriteMode.Merge:
                if (_context.Root[_context.SectionName] is JObject existingSection) {
                    foreach (var prop in _context.Current.Properties())
                        existingSection[prop.Name] = prop.Value;
                    _context.Root[_context.SectionName] = existingSection;
                }
                else {
                    _context.Root[_context.SectionName] = _context.Current;
                }

                break;

            case SectionWriteMode.AddOnly:
                if (_context.Root[_context.SectionName] is JObject existingSection2) {
                    foreach (var prop in _context.Current.Properties())
                        if (!existingSection2.TryGetValue(prop.Name, out _))
                            existingSection2[prop.Name] = prop.Value;
                    _context.Root[_context.SectionName] = existingSection2;
                }
                else {
                    _context.Root[_context.SectionName] = _context.Current;
                }

                break;
            default:
                _context.Root[_context.SectionName] = _context.Current;
                break;
        }

        var context = _context;
        _context = null;
        
        File.WriteAllText(context.FilePath, context.Root.ToString(Formatting.Indented));
    }

    public void RemoveOtherSections() {
        if (_context == null)
            throw new InvalidOperationException("Call StartWrite first");

        var keysToRemove = _context.Root.Properties()
            .Where(p => p.Name != _context.SectionName)
            .Select(p => p.Name)
            .ToList();

        foreach (var key in keysToRemove)
            _context.Root.Remove(key);
    }

    public void RemoveSection(string sectionName) {
        if (_context == null)
            throw new InvalidOperationException("Call StartWrite first");

        _context.Root.Remove(sectionName);
    }

    public void StartWrite(string fileName, string sectionName, SectionWriteMode sectionWriteMode = SectionWriteMode.Replace) {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));

        if (string.IsNullOrEmpty(sectionName))
            throw new ArgumentNullException(nameof(sectionName));

        if (_context != null)
            throw new InvalidOperationException("A write operation is already in progress");

        _sectionWriteMode = sectionWriteMode;
        Directory.CreateDirectory(_baseDir);
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
                    root = token as JObject ?? new JObject();
                }
                catch (Exception e) {
                    LogManager.GetLogger().Error(e, $"{nameof(NewtonsoftWriterAdapter)}.{nameof(StartWrite)}: Failed to parse JSON file: {filePath}, json string: {json}\n Starting with an empty object.");
                    root = new JObject();
                }
            }
        }

        _context = new SerializationContext(filePath, sectionName, root, new JObject());
    }

    public void WriteProperty<T>(string name, T value) {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (_context == null)
            throw new InvalidOperationException("Call StartWrite first");

        _context.Current[name] = value != null ? JToken.FromObject(value) : JValue.CreateNull();
    }
}