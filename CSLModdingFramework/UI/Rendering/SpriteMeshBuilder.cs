using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public static class SpriteMeshBuilder {
    public static readonly int[] QuadTriangleIndices = {
        0, 1, 3,
        3, 1, 2
    };

    public static void RenderSprite(UIRenderData data, SpriteRenderData spriteRenderData) {
        spriteRenderData.BaseIndex = data.vertices.Count;
        RebuildTriangles(data, spriteRenderData);
        RebuildVertices(data, spriteRenderData);
        RebuildUV(data, spriteRenderData);
        RebuildColors(data, spriteRenderData);
        if (spriteRenderData.FillAmount < 1f) {
            DoFill(data, spriteRenderData);
        }
    }

    private static void RebuildTriangles(UIRenderData renderData, SpriteRenderData spriteRenderData) {
        var baseIndex = spriteRenderData.BaseIndex;
        var triangles = renderData.triangles;

        triangles.EnsureCapacity(triangles.Count + QuadTriangleIndices.Length);

        for (var i = 0; i < QuadTriangleIndices.Length; i++) {
            triangles.Add(baseIndex + QuadTriangleIndices[i]);
        }
    }

    private static void RebuildVertices(UIRenderData renderData, SpriteRenderData spriteRenderData) {
        var vertices = renderData.vertices;
        var baseIndex = spriteRenderData.BaseIndex;

        const float left = 0f;
        const float top = 0f;
        var right = Mathf.Ceil(spriteRenderData.Size.x);
        var bottom = Mathf.Ceil(-spriteRenderData.Size.y);

        var scale = spriteRenderData.UnitsPerPixel;

        vertices.Add(new Vector3(left, top, 0f) * scale);
        vertices.Add(new Vector3(right, top, 0f) * scale);
        vertices.Add(new Vector3(right, bottom, 0f) * scale);
        vertices.Add(new Vector3(left, bottom, 0f) * scale);

        var offset = spriteRenderData.Offset.Scale(1, -1, 1).RoundToInt() * scale;

        for (var i = 0; i < 4; i++) {
            vertices[baseIndex + i] =
                (vertices[baseIndex + i] + offset)
                .Quantize(scale);
        }
    }


    private static void RebuildColors(UIRenderData renderData, SpriteRenderData spriteRenderData) {
        Color32 vertexColor = ((Color)spriteRenderData.Color).linear;
        var colors = renderData.colors;

        colors.Add(vertexColor);
        colors.Add(vertexColor);
        colors.Add(vertexColor);
        colors.Add(vertexColor);
    }


    private static void RebuildUV(UIRenderData renderData, SpriteRenderData spriteRenderData) {
        var region = spriteRenderData.SpriteMetadata.Region;
        var uvs = renderData.uvs;

        uvs.Add(new Vector2(region.x, region.yMax));
        uvs.Add(new Vector2(region.xMax, region.yMax));
        uvs.Add(new Vector2(region.xMax, region.y));
        uvs.Add(new Vector2(region.x, region.y));

        if (spriteRenderData.Flip.IsFlagSet(UISpriteFlip.FlipHorizontal)) {
            SwapUV(uvs, 0, 1);
            SwapUV(uvs, 3, 2);
        }

        if (spriteRenderData.Flip.IsFlagSet(UISpriteFlip.FlipVertical)) {
            SwapUV(uvs, 0, 3);
            SwapUV(uvs, 1, 2);
        }
    }

    private static void SwapUV(PoolList<Vector2> list, int a, int b) => (list[a], list[b]) = (list[b], list[a]);


    private static void DoFill(UIRenderData renderData, SpriteRenderData spriteRenderData) {
        var baseIndex = spriteRenderData.BaseIndex;
        var vertices = renderData.vertices;
        var uvs = renderData.uvs;

        var lt = baseIndex + 0;
        var rt = baseIndex + 1;
        var rb = baseIndex + 2;
        var lb = baseIndex + 3;

        int a, b, c, d;

        var horizontal = spriteRenderData.FillDirection == UIFillDirection.Horizontal;

        if (horizontal) {
            a = lt;
            b = lb;
            c = rt;
            d = rb;

            if (spriteRenderData.InvertFill) {
                a = rt;
                b = rb;
                c = lt;
                d = lb;
            }
        }
        else {
            a = lb;
            b = rb;
            c = lt;
            d = rt;

            if (spriteRenderData.InvertFill) {
                a = lt;
                b = rt;
                c = lb;
                d = rb;
            }
        }

        var t = 1f - spriteRenderData.FillAmount;

        vertices[c] = Vector3.Lerp(vertices[c], vertices[a], t);
        vertices[d] = Vector3.Lerp(vertices[d], vertices[b], t);

        uvs[c] = Vector2.Lerp(uvs[c], uvs[a], t);
        uvs[d] = Vector2.Lerp(uvs[d], uvs[b], t);
    }
}