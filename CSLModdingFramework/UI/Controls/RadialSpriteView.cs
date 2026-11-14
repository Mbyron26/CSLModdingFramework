using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class RadialSpriteView : SpriteView {
    private static readonly Vector3[] BaseVerts = {
        new(0f, 0.5f, 0f),
        new(0.5f, 0.5f, 0f),
        new(0.5f, 0f, 0f),
        new(0.5f, -0.5f, 0f),
        new(0f, -0.5f, 0f),
        new(-0.5f, -0.5f, 0f),
        new(-0.5f, 0f, 0f),
        new(-0.5f, 0.5f, 0f)
    };

    protected UIRenderData _radialRenderData;
    protected UIPivotPoint _fillOrigin = UIPivotPoint.MiddleCenter;

    public UIPivotPoint FillOrigin {
        get => _fillOrigin;
        set {
            if (value == _fillOrigin) return;
            _fillOrigin = value;
            Invalidate();
        }
    }

    protected override void OnRebuildRenderData() {
        if (_atlas == null) return;
        if (_radialRenderData is not null) {
            _radialRenderData.Clear();
        }
        else {
            _radialRenderData = UIRenderData.Obtain();
            RenderData.Add(_radialRenderData);
        }

        _radialRenderData.material = _atlas.Material;

        if (string.IsNullOrEmpty(_spriteName)) return;

        if (!_atlas.TryGetSprite(_spriteName, out int _)) return;

        List<Vector3> list = null;
        List<int> list2 = null;
        List<Vector2> list3 = null;
        BuildMeshData(ref list, ref list2, ref list3);
        var array = BuildColors(list.Count);
        _radialRenderData.vertices.AddRange(list);
        _radialRenderData.triangles.AddRange(list2);
        _radialRenderData.uvs.AddRange(list3);
        _radialRenderData.colors.AddRange(array);
    }

    private Color32[] BuildColors(int vertCount) {
        Color32 renderColor = ((Color)Colors.GetValue(_state)).linear;
        var array = new Color32[vertCount];
        for (var i = 0; i < array.Length; i++) {
            array[i] = renderColor;
        }

        return array;
    }

    private void BuildMeshData(ref List<Vector3> vertices, ref List<int> indices, ref List<Vector2> uvs) {
        vertices = new List<Vector3>();
        vertices.AddRange(BaseVerts);
        int num;
        int num2;
        switch (FillOrigin) {
            case UIPivotPoint.TopLeft:
                num = 4;
                num2 = 5;
                vertices.RemoveAt(6);
                vertices.RemoveAt(0);
                break;
            case UIPivotPoint.TopCenter:
                num = 6;
                num2 = 0;
                break;
            case UIPivotPoint.TopRight:
                num = 4;
                num2 = 0;
                vertices.RemoveAt(2);
                vertices.RemoveAt(0);
                break;
            case UIPivotPoint.MiddleLeft:
                num = 6;
                num2 = 6;
                break;
            case UIPivotPoint.MiddleCenter:
                num = 8;
                vertices.Add(vertices[0]);
                vertices.Insert(0, Vector3.zero);
                num2 = 0;
                break;
            case UIPivotPoint.MiddleRight:
                num = 6;
                num2 = 2;
                break;
            case UIPivotPoint.BottomLeft:
                num = 4;
                num2 = 4;
                vertices.RemoveAt(6);
                vertices.RemoveAt(4);
                break;
            case UIPivotPoint.BottomCenter:
                num = 6;
                num2 = 4;
                break;
            case UIPivotPoint.BottomRight:
                num = 4;
                num2 = 2;
                vertices.RemoveAt(4);
                vertices.RemoveAt(2);
                break;
            default:
                throw new NotImplementedException();
        }

        StartClosestToPivot(vertices, num2);
        List<int> list;
        indices = (list = BuildTriangles(vertices));
        var list2 = list;
        var num3 = 1f / num;
        var num4 = FillAmount.Quantize(num3);
        for (var i = Mathf.CeilToInt(num4 / num3) + 1; i < num; i++) {
            if (InvertFill) {
                list2.RemoveRange(0, 3);
            }
            else {
                vertices.RemoveAt(vertices.Count - 1);
                list2.RemoveRange(list2.Count - 3, 3);
            }
        }

        if (FillAmount < 1f) {
            var num5 = list2[InvertFill ? 2 : (list2.Count - 2)];
            var num6 = list2[InvertFill ? 1 : (list2.Count - 1)];
            var num7 = (FillAmount - num4) / num3;
            vertices[num6] = Vector3.Lerp(vertices[num5], vertices[num6], num7);
        }

        uvs = BuildUV(vertices);
        var num8 = PixelsToUnits();
        Vector3 vector = num8 * size;
        var vector2 = pivot.TransformToCenter(size, arbitraryPivotOffset) * num8;
        for (var j = 0; j < vertices.Count; j++) {
            vertices[j] = Vector3.Scale(vertices[j], vector) + vector2;
        }
    }

    private void StartClosestToPivot(List<Vector3> list, int index) {
        if (index == 0) {
            return;
        }

        var range = list.GetRange(index, list.Count - index);
        list.RemoveRange(index, list.Count - index);
        list.InsertRange(0, range);
    }

    private List<int> BuildTriangles(List<Vector3> vertices) {
        var list = new List<int>();
        var count = vertices.Count;
        for (var i = 1; i < count - 1; i++) {
            list.Add(0);
            list.Add(i);
            list.Add(i + 1);
        }

        return list;
    }

    private List<Vector2> BuildUV(List<Vector3> vertices) {
        _atlas.TryGetSprite(_spriteName, out int spriteIndex);
        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

        var region = spriteMetadata.Region;
        if (Flip.IsFlagSet(UISpriteFlip.FlipHorizontal)) {
            region = new Rect(region.xMax, region.y, -region.width, region.height);
        }

        if (Flip.IsFlagSet(UISpriteFlip.FlipVertical)) {
            region = new Rect(region.x, region.yMax, region.width, -region.height);
        }

        var vector = new Vector2(region.x, region.y);
        var vector2 = new Vector2(0.5f, 0.5f);
        var vector3 = new Vector2(region.width, region.height);
        var list = new List<Vector2>(vertices.Count);
        for (var i = 0; i < vertices.Count; i++) {
            var vector4 = (Vector2)vertices[i] + vector2;
            list.Add(Vector2.Scale(vector4, vector3) + vector);
        }

        return list;
    }
}