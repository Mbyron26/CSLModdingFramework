using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public struct SpriteRenderData {
    public IAtlas Atlas;
    public AtlasSpriteMetadata SpriteMetadata;
    public Color32 Color;
    public float UnitsPerPixel;
    public Vector2 Size;
    public UISpriteFlip Flip;
    public bool InvertFill;
    public UIFillDirection FillDirection;
    public float FillAmount;
    public Vector3 Offset;
    public int BaseIndex;
}