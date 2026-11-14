using System;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Fonts;

public sealed class DynamicFont {
    internal static Shader FontShaderInternal;
    private Material _material;

    public int FontSize => 16;
    public int LineHeight => 22;
    public int Baseline => 18;
    public int Descent => LineHeight - Baseline;
    public Font BaseFont { get; }

    public Material FontMaterial {
        get {
            if (_material == null) return _material;

            _material.mainTexture = BaseFont.material.mainTexture;
            _material.shader = FontShader;
            return _material;
        }
    }

    public Shader FontShader => FontShaderInternal ??= Shader.Find("UI/Dynamic Font Shader");
    public Texture FontTexture => BaseFont.material.mainTexture;

    public bool IsValid => BaseFont != null && FontMaterial != null && FontTexture != null;

    public DynamicFont(UIDynamicFont uiDynamicFont) {
        BaseFont = uiDynamicFont.baseFont;
        _material = new Material(uiDynamicFont.material);
    }

    public DynamicFontRenderer RentRenderer() => DynamicFontRenderer.Rent(this);

    public void RequestCharacters(string text, int size, FontStyle style) {
        if (BaseFont == null) {
            throw new NullReferenceException("Base Font not assigned.");
        }

        BaseFont.RequestCharactersInTexture(text, size, style);
    }

    public void GetCharacterInfo(char ch, out CharacterInfo info, float scale) {
        if (BaseFont == null) {
            throw new NullReferenceException("Base Font not assigned.");
        }

        BaseFont.GetCharacterInfo(ch, out info, Mathf.CeilToInt(FontSize * scale), FontStyle.Normal);
    }
}