using UnityEngine;

namespace CSLModdingFramework.UI;

public interface IAtlas {
    bool TryGetSprite(string name, out AtlasSpriteMetadata meta);
    bool TryGetSprite(string name, out int spriteIndex);
    ref readonly AtlasSpriteMetadata GetSprite(int index);
    Texture2D Texture { get; }
    Material Material { get; }
}