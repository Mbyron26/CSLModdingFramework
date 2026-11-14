using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ColossalFramework.UI;
using CSLModdingFramework.Logging;
using CSLModdingFramework.UI.Utilities;
using CSLModdingFramework.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using ArrayExtensions = CSLModdingFramework.Extensions.ArrayExtensions;

namespace CSLModdingFramework.UI;

public class TextureAtlas : IAtlas {
    private static readonly Dictionary<(string Name, Type Type), TextureAtlas> AtlasMap = new();

    protected readonly ILog _logger = LogManager.GetLogger();
    private readonly Dictionary<string, int> _spriteIndex = new();

    private readonly Dictionary<string, SpriteEntry> _sprites = new();
    private AtlasSpriteMetadata[] _spriteArray = ArrayExtensions.Empty<AtlasSpriteMetadata>();

    protected TextureAtlas() { }

    protected TextureAtlas(string name) => AtlasName = name ?? throw new ArgumentNullException(nameof(name));

    public virtual string ResourcePath { get; set; }
    protected virtual Assembly ResourceAssembly { get; }
    protected virtual AtlasResourceSource ResourceSource => AtlasResourceSource.Assembly;

    public string AtlasName { get; protected set; }
    public Material Material { get; protected set; }

    public Texture2D Texture { get; protected set; }

    protected virtual void RegisterSprites() { }

    public static TextureAtlas Create(string name) {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var key = (name, typeof(TextureAtlas));

        if (AtlasMap.TryGetValue(key, out var atlas))
            return atlas;

        atlas = new TextureAtlas(name);
        atlas.Build();
        AtlasMap[key] = atlas;
        return atlas;
    }

    public static T Create<T>(string name) where T : TextureAtlas, new() {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var key = (name, typeof(T));

        if (AtlasMap.TryGetValue(key, out var atlas))
            return (T)atlas;

        atlas = new T {
            AtlasName = name
        };
        atlas.Build();
        AtlasMap[key] = atlas;
        return (T)atlas;
    }

    public static UITextureAtlas GetAtlas(string atlasName) {
        var atlases = GetAtlases();

        foreach (var atlas in atlases) {
            if (atlas.name != atlasName)
                continue;

            LogManager.GetLogger().Info($"Obtained {atlasName} UITextureAtlas.");
            return atlas;
        }

        return null;
    }

    public static IEnumerable<UITextureAtlas> GetAtlases() {
        if (Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) is not UITextureAtlas[] atlases)
            yield break;

        foreach (var atlas in atlases)
            yield return atlas;
    }

    public void AddSprite(string name, Texture2D texture, SpriteSlice slice) {
        if (string.IsNullOrEmpty(name) || texture == null) {
            _logger.Warn("AddSprite received invalid parameters.");
            return;
        }

        if (_sprites.TryGetValue(name, out var old) && old.External && old.Texture != null)
            Object.Destroy(old.Texture);

        _sprites[name] = new SpriteEntry {
            Texture = CloneTexture(texture),
            Slice = slice,
            External = true
        };
    }

    public void AddSprite(UITextureAtlas.SpriteInfo info) {
        if (info == null) {
            _logger.Warn("AddSprite(SpriteInfo) received null info.");
            return;
        }

        var name = info.name ?? "UnnamedSprite";
        var slice = new SpriteSlice(info.border.left, info.border.right, info.border.top, info.border.bottom);

        AddSprite(name, info.texture, slice);
        _logger.Verbose($"Added sprite {name} to {AtlasName}.");
    }

    public void Build(int maxSize = 1024) {
        RegisterSprites();

        if (Material == null)
            Material = Object.Instantiate(UIView.GetAView().defaultAtlas.material);

        PackAtlas(maxSize);
    }

    public ref readonly AtlasSpriteMetadata GetSprite(int index) => ref _spriteArray[index];

    public bool TryGetSprite(string name, out int index) {
        if (name != null) return _spriteIndex.TryGetValue(name, out index);

        index = -1;
        return false;
    }

    public bool TryGetSprite(string name, out AtlasSpriteMetadata meta) {
        if (_spriteIndex.TryGetValue(name, out var index)) {
            meta = _spriteArray[index];
            return true;
        }

        meta = default;
        return false;
    }

    protected void AddInternalSprite(string name, SpriteSlice slice) =>
        _sprites[name] = new SpriteEntry {
            Texture = null,
            Slice = slice,
            External = false
        };

    protected Texture2D CreatePlaceholderTexture() => TextureLoader.CreateTexture(32, 32, Color.magenta);

    protected Texture2D LoadTexture(string name) {
        try {
            if (string.IsNullOrEmpty(ResourcePath)) {
                _logger.Warn($"ResourcePath empty for {name}, using placeholder.");
                return CreatePlaceholderTexture();
            }

            switch (ResourceSource) {
                case AtlasResourceSource.Assembly:
                default:
                    var res = $"{ResourcePath}.{name}.png";
                    var tex = TextureLoader.FromAssembly(res, ResourceAssembly);
                    if (tex != null) return tex;

                    _logger.Warn($"Missing embedded resource: {res}");
                    return CreatePlaceholderTexture();

                case AtlasResourceSource.File:
                    var file = Path.Combine(ResourcePath, name + ".png");
                    if (File.Exists(file))
                        return TextureLoader.FromFileSafe(file);

                    _logger.Warn($"Missing file resource: {file}");
                    return CreatePlaceholderTexture();
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, $"LoadTexture failed for {name}");
            return CreatePlaceholderTexture();
        }
    }

    protected void PackAtlas(int maxSize = 1024) {
        using var t = PerformanceCounter.Start(v =>
            _logger.Debug($"PackAtlas {AtlasName} took {v.Milliseconds}ms"));

        try {
            if (Texture != null) {
                Object.Destroy(Texture);
                Texture = null;
            }

            Texture = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);

            var pairs = _sprites.ToArray();

            var count = pairs.Length;

            var textures = new Texture2D[count];

            for (var i = 0; i < count; i++) {
                var kv = pairs[i];

                var entry = kv.Value;

                textures[i] = entry.External
                    ? entry.Texture
                    : LoadTexture(kv.Key) ?? CreatePlaceholderTexture();
            }

            var rects = Texture.PackTextures(textures, 2, maxSize);

            if (rects.Length != count) {
                _logger.Error($"Atlas {AtlasName} overflow: {count} sprites, only {rects.Length} packed.");

                UseFallbackAtlas();
                return;
            }

            Material.mainTexture = Texture;

            _spriteArray = new AtlasSpriteMetadata[count];
            _spriteIndex.Clear();

            for (var i = 0; i < count; i++) {
                var kv = pairs[i];
                var entry = kv.Value;
                var tex = textures[i];

                _spriteArray[i] = new AtlasSpriteMetadata(
                    kv.Key,
                    rects[i],
                    new Vector2(tex.width, tex.height),
                    entry.Slice
                );

                _spriteIndex[kv.Key] = i;
            }

            foreach (var tex in textures)
                if (tex != null && tex != Texture)
                    Object.Destroy(tex);
        }
        catch (Exception ex) {
            _logger.Error(ex, $"PackAtlas failed for {AtlasName}, using fallback.");
            UseFallbackAtlas();
        }
    }

    private Texture2D CloneTexture(Texture2D src) {
        try {
            var copy = new Texture2D(
                src.width,
                src.height,
                src.format,
                src.mipmapCount > 1
            );
            Debug.Log($"Cloning texture: {src.name}, width: {src.width}, height: {src.height}, format: {src.format}, mipmapCount: {src.mipmapCount}");

            Graphics.CopyTexture(src, copy);
            copy.filterMode = src.filterMode;
            copy.wrapMode = src.wrapMode;
            copy.anisoLevel = src.anisoLevel;
            return copy;
        }
        catch (Exception ex) {
            _logger.Error(ex, "CloneTexture failed.");
            return CreatePlaceholderTexture();
        }
    }

    private void UseFallbackAtlas() {
        if (Texture != null)
            Object.Destroy(Texture);

        Texture = CreatePlaceholderTexture();
        Material.mainTexture = Texture;

        _spriteArray = ArrayExtensions.Empty<AtlasSpriteMetadata>();
        _spriteIndex.Clear();
    }

    protected struct SpriteEntry {
        public bool External;
        public SpriteSlice Slice;
        public Texture2D Texture;
    }
}