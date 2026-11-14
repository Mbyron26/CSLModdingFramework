using UnityEngine;

namespace CSLModdingFramework.UI;

public readonly struct AtlasSpriteMetadata {
    public readonly string Name;
    public readonly Vector2 PixelSize;
    public readonly Rect Region;
    public readonly SpriteSlice Slice;

    public AtlasSpriteMetadata(string name, Rect region, Vector2 pixelSize, SpriteSlice slice) {
        Name = name;
        Region = region;
        PixelSize = pixelSize;
        Slice = slice;
    }

    public bool IsSliced => Slice.IsValid;
}