using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class TextureSpriteView : UIControl {
    protected Texture _sourceTexture;
    protected Material _sourceMaterial;
    protected UISpriteFlip _flip;
    protected UIFillDirection _fillDirection;
    protected float _fillAmount = 1f;
    protected bool _invertFill;
    protected Color32 _normalColor = UIColorPalette.White;
    protected Color32 _disabledColor = UIColorPalette.White;
    private Material _renderMaterial;

    public event UIEventHandler<TextureSpriteView, Texture> TextureChanged;

    public Color32 NormalColor {
        get => _normalColor;
        set {
            if (value.EqualsColor(_normalColor)) return;

            _normalColor = value;
            Invalidate();
        }
    }

    public Color32 DisabledColor {
        get => _disabledColor;
        set {
            if (value.EqualsColor(_disabledColor)) return;

            _disabledColor = value;
            Invalidate();
        }
    }

    public Texture SourceTexture {
        get => _sourceTexture;
        set {
            if (value == _sourceTexture) return;

            _sourceTexture = value;
            Invalidate();
            if (value != null && size == Vector2.zero) {
                size = new Vector2(value.width, value.height);
            }

            OnTextureChanged(value);
        }
    }

    public Material SourceMaterial {
        get => _sourceMaterial;
        set {
            if (value == _sourceMaterial) return;

            if (_renderMaterial) {
                DestroyImmediate(_renderMaterial);
            }

            _renderMaterial = null;
            _sourceMaterial = value;
            Invalidate();
        }
    }

    public UISpriteFlip Flip {
        get => _flip;
        set {
            if (value == _flip) return;

            _flip = value;
            Invalidate();
        }
    }

    public UIFillDirection FillDirection {
        get => _fillDirection;
        set {
            if (value == _fillDirection) return;

            _fillDirection = value;
            Invalidate();
        }
    }

    public float FillAmount {
        get => _fillAmount;
        set {
            if (Mathf.Approximately(value, _fillAmount)) return;

            _fillAmount = Mathf.Max(0f, Mathf.Min(1f, value));
            Invalidate();
        }
    }

    public bool InvertFill {
        get => _invertFill;
        set {
            if (value == _invertFill) return;

            _invertFill = value;
            Invalidate();
        }
    }

    public Material RenderMaterial {
        get {
            if (_renderMaterial != null) return _renderMaterial;

            _renderMaterial = new Material(Shader.Find("UI/Default UI Shader")) {
                shader = _sourceMaterial.shader,
                hideFlags = HideFlags.DontSave,
                name = _sourceMaterial.name + " (Material Copy)"
            };
            _renderMaterial.CopyPropertiesFromMaterial(_sourceMaterial);

            return _renderMaterial;
        }
    }

    protected virtual void OnTextureChanged(Texture value) => TextureChanged?.Invoke(this, value);

    public override void OnDisable() {
        if (_renderMaterial) {
            DestroyImmediate(_renderMaterial);
        }

        _renderMaterial = null;
        base.OnDisable();
    }

    protected override void OnRebuildRenderData() {
        if (_sourceTexture == null) return;

        EnsureMaterial();
        if (_sourceMaterial == null) return;

        RenderMaterial.mainTexture = _sourceTexture;
        renderData.material = RenderMaterial;
        renderData.uniqueMaterial = true;
        var unitsPerPixel = PixelsToUnits();
        var num4 = size.x * unitsPerPixel;
        var num5 = -size.y * unitsPerPixel;
        var vector = pivot.TransformToUpperLeft(size, arbitraryPivotOffset).RoundToInt() * unitsPerPixel;
        renderData.vertices.Add(new Vector3(0, 0, 0f) + vector);
        renderData.vertices.Add(new Vector3(num4, 0, 0f) + vector);
        renderData.vertices.Add(new Vector3(num4, num5, 0f) + vector);
        renderData.vertices.Add(new Vector3(0, num5, 0f) + vector);
        renderData.triangles.AddRange(SpriteMeshBuilder.QuadTriangleIndices);
        RebuildUV(renderData);
        var renderColor = isEnabled ? _normalColor : _disabledColor;
        renderData.colors.Add(renderColor);
        renderData.colors.Add(renderColor);
        renderData.colors.Add(renderColor);
        renderData.colors.Add(renderColor);
        if (FillAmount < 1f) {
            DoFill(renderData);
        }
    }

    private void EnsureMaterial() {
        if (_sourceMaterial != null || _sourceTexture == null) return;

        var uiView = GetUIView();
        if (uiView == null || uiView.defaultAtlas == null || uiView.defaultAtlas.material == null) return;

        _sourceMaterial = new Material(GetUIView().defaultAtlas.material);
    }

    private void RebuildUV(UIRenderData renderMesh) {
        var uvs = renderMesh.uvs;

        var uvTopLeft = new Vector2(0f, 1f);
        var uvTopRight = new Vector2(1f, 1f);
        var uvBottomRight = new Vector2(1f, 0f);
        var uvBottomLeft = new Vector2(0f, 0f);

        if (Flip.IsFlagSet(UISpriteFlip.FlipHorizontal)) {
            (uvTopLeft, uvTopRight) = (uvTopRight, uvTopLeft);
            (uvBottomLeft, uvBottomRight) = (uvBottomRight, uvBottomLeft);
        }

        if (Flip.IsFlagSet(UISpriteFlip.FlipVertical)) {
            (uvTopLeft, uvBottomLeft) = (uvBottomLeft, uvTopLeft);
            (uvTopRight, uvBottomRight) = (uvBottomRight, uvTopRight);
        }

        uvs.Add(uvTopLeft);
        uvs.Add(uvTopRight);
        uvs.Add(uvBottomRight);
        uvs.Add(uvBottomLeft);
    }

    private void DoFill(UIRenderData renderMesh) {
        var vertices = renderMesh.vertices;
        var uvs = renderMesh.uvs;

        const int topLeft = 0;
        const int topRight = 1;
        const int bottomRight = 2;
        const int bottomLeft = 3;

        int cutStartA, cutStartB, cutEndA, cutEndB;

        if (FillDirection == UIFillDirection.Horizontal) {
            if (!InvertFill) {
                cutStartA = topRight;
                cutStartB = bottomRight;
                cutEndA = topLeft;
                cutEndB = bottomLeft;
            }
            else {
                cutStartA = topLeft;
                cutStartB = bottomLeft;
                cutEndA = topRight;
                cutEndB = bottomRight;
            }
        }
        else {
            if (!InvertFill) {
                cutStartA = bottomLeft;
                cutStartB = bottomRight;
                cutEndA = topLeft;
                cutEndB = topRight;
            }
            else {
                cutStartA = topLeft;
                cutStartB = topRight;
                cutEndA = bottomLeft;
                cutEndB = bottomRight;
            }
        }

        var t = 1f - FillAmount;

        vertices[cutStartA] = Vector3.Lerp(vertices[cutStartA], vertices[cutEndA], t);
        vertices[cutStartB] = Vector3.Lerp(vertices[cutStartB], vertices[cutEndB], t);

        uvs[cutStartA] = Vector2.Lerp(uvs[cutStartA], uvs[cutEndA], t);
        uvs[cutStartB] = Vector2.Lerp(uvs[cutStartB], uvs[cutEndB], t);
    }
}