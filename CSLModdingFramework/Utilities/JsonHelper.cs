using Newtonsoft.Json;

namespace CSLModdingFramework.Utilities;

public static class JsonHelper {
    public static readonly JsonSerializerSettings Settings = new() {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    public static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Settings);

    public static string Serialize(object obj) => JsonConvert.SerializeObject(obj, Settings);
}