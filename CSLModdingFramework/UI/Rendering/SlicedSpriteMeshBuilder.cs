using System;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public static class SlicedSpriteMeshBuilder {
    public static readonly int[] NineSliceTriangleIndices = {
        0, 1, 2, 2, 3, 0,
        4, 5, 6, 6, 7, 4,
        8, 9, 10, 10, 11, 8,

        12, 13, 14, 14, 15, 12,
        1, 4, 7, 7, 2, 1,
        9, 12, 15, 15, 10, 9,

        3, 2, 9, 9, 8, 3,
        7, 6, 13, 13, 12, 7,
        2, 7, 12, 12, 9, 2
    };

    private static readonly int[][] HorizontalFillVertexGroups = {
        new[] { 0, 1, 4, 5 },
        new[] { 3, 2, 7, 6 },
        new[] { 8, 9, 12, 13 },
        new[] { 11, 10, 15, 14 }
    };

    private static readonly int[][] VerticalFillVertexGroups = {
        new[] { 11, 8, 3, 0 },
        new[] { 10, 9, 2, 1 },
        new[] { 15, 12, 7, 4 },
        new[] { 14, 13, 6, 5 }
    };

    private static readonly int[][] ActiveFillVertexGroups = {
        new int[4],
        new int[4],
        new int[4],
        new int[4]
    };

    private static readonly Vector3[] SliceVertices = new Vector3[16];

    private static readonly Vector2[] SliceUVs = new Vector2[16];


    public static void RenderSprite(UIRenderData renderData, SpriteRenderData spriteRenderData) {
        spriteRenderData.BaseIndex = renderData.vertices.Count;

        RebuildTriangles(renderData, spriteRenderData);
        RebuildVertices(renderData, spriteRenderData);
        RebuildUV(renderData, spriteRenderData);
        RebuildColors(renderData, spriteRenderData);

        if (spriteRenderData.FillAmount < 1f) {
            DoFill(renderData, spriteRenderData);
        }
    }

    private static void RebuildTriangles(UIRenderData renderData, SpriteRenderData spriteRenderData) {
        var baseIndex = spriteRenderData.BaseIndex;
        var triangles = renderData.triangles;

        for (var i = 0; i < NineSliceTriangleIndices.Length; i++) {
            triangles.Add(baseIndex + NineSliceTriangleIndices[i]);
        }
    }

    private static void RebuildVertices(UIRenderData renderData, SpriteRenderData sprite) {
        var offset = sprite.Offset.Scale(1, -1, 1);
        var slice = sprite.SpriteMetadata.Slice;
        var size = sprite.Size;
        var scale = sprite.UnitsPerPixel;

        var left = 0f;
        var top = 0f;
        var right = Mathf.Ceil(size.x);
        var bottom = Mathf.Ceil(-size.y);

        float sliceL = slice.Left;
        float sliceT = slice.Top;
        float sliceR = slice.Right;
        float sliceB = slice.Bottom;

        if (sprite.Flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
            (sliceL, sliceR) = (sliceR, sliceL);

        if (sprite.Flip.IsFlagSet(UISpriteFlip.FlipVertical))
            (sliceT, sliceB) = (sliceB, sliceT);

        SliceVertices[0] = new Vector3(left, top, 0) + offset;
        SliceVertices[1] = SliceVertices[0] + new Vector3(sliceL, 0, 0);
        SliceVertices[2] = SliceVertices[0] + new Vector3(sliceL, -sliceT, 0);
        SliceVertices[3] = SliceVertices[0] + new Vector3(0, -sliceT, 0);

        SliceVertices[4] = new Vector3(right - sliceR, top, 0) + offset;
        SliceVertices[5] = SliceVertices[4] + new Vector3(sliceR, 0, 0);
        SliceVertices[6] = SliceVertices[4] + new Vector3(sliceR, -sliceT, 0);
        SliceVertices[7] = SliceVertices[4] + new Vector3(0, -sliceT, 0);

        SliceVertices[8] = new Vector3(left, bottom + sliceB, 0) + offset;
        SliceVertices[9] = SliceVertices[8] + new Vector3(sliceL, 0, 0);
        SliceVertices[10] = SliceVertices[8] + new Vector3(sliceL, -sliceB, 0);
        SliceVertices[11] = SliceVertices[8] + new Vector3(0, -sliceB, 0);

        SliceVertices[12] = new Vector3(right - sliceR, bottom + sliceB, 0) + offset;
        SliceVertices[13] = SliceVertices[12] + new Vector3(sliceR, 0, 0);
        SliceVertices[14] = SliceVertices[12] + new Vector3(sliceR, -sliceB, 0);
        SliceVertices[15] = SliceVertices[12] + new Vector3(0, -sliceB, 0);

        for (var i = 0; i < 16; i++)
            renderData.vertices.Add((SliceVertices[i] * scale).Quantize(scale));
    }

    private static void RebuildUV(UIRenderData renderData, SpriteRenderData sprite) {
        var atlasSize = new Vector2(sprite.Atlas.Texture.width, sprite.Atlas.Texture.height);
        var region = sprite.SpriteMetadata.Region;
        var slice = sprite.SpriteMetadata.Slice;

        var uvL = slice.Left / atlasSize.x;
        var uvR = slice.Right / atlasSize.x;
        var uvT = slice.Top / atlasSize.y;
        var uvB = slice.Bottom / atlasSize.y;

        SliceUVs[0] = new Vector2(region.x, region.yMax);
        SliceUVs[1] = new Vector2(region.x + uvL, region.yMax);
        SliceUVs[2] = new Vector2(region.x + uvL, region.yMax - uvT);
        SliceUVs[3] = new Vector2(region.x, region.yMax - uvT);

        SliceUVs[4] = new Vector2(region.xMax - uvR, region.yMax);
        SliceUVs[5] = new Vector2(region.xMax, region.yMax);
        SliceUVs[6] = new Vector2(region.xMax, region.yMax - uvT);
        SliceUVs[7] = new Vector2(region.xMax - uvR, region.yMax - uvT);

        SliceUVs[8] = new Vector2(region.x, region.y + uvB);
        SliceUVs[9] = new Vector2(region.x + uvL, region.y + uvB);
        SliceUVs[10] = new Vector2(region.x + uvL, region.y);
        SliceUVs[11] = new Vector2(region.x, region.y);

        SliceUVs[12] = new Vector2(region.xMax - uvR, region.y + uvB);
        SliceUVs[13] = new Vector2(region.xMax, region.y + uvB);
        SliceUVs[14] = new Vector2(region.xMax, region.y);
        SliceUVs[15] = new Vector2(region.xMax - uvR, region.y);

        if (sprite.Flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
            SwapColumns(SliceUVs, 0, 1, 2, 3);

        if (sprite.Flip.IsFlagSet(UISpriteFlip.FlipVertical))
            SwapRows(SliceUVs, 0, 1, 2, 3);

        for (var i = 0; i < 16; i++)
            renderData.uvs.Add(SliceUVs[i]);
    }

    private static void SwapColumns(Vector2[] uv, int c0, int c1, int c2, int c3) {
        for (var row = 0; row < 4; row++)
            (uv[row * 4 + c0], uv[row * 4 + c3]) = (uv[row * 4 + c3], uv[row * 4 + c0]);

        for (var row = 0; row < 4; row++)
            (uv[row * 4 + c1], uv[row * 4 + c2]) = (uv[row * 4 + c2], uv[row * 4 + c1]);
    }

    private static void SwapRows(Vector2[] uv, int r0, int r1, int r2, int r3) {
        for (var col = 0; col < 4; col++)
            (uv[r0 * 4 + col], uv[r3 * 4 + col]) = (uv[r3 * 4 + col], uv[r0 * 4 + col]);

        for (var col = 0; col < 4; col++)
            (uv[r1 * 4 + col], uv[r2 * 4 + col]) = (uv[r2 * 4 + col], uv[r1 * 4 + col]);
    }


    private static void RebuildColors(UIRenderData renderData, SpriteRenderData spriteRenderData) {
        Color32 vertexColor = ((Color)spriteRenderData.Color).linear;
        for (var i = 0; i < 16; i++) {
            renderData.colors.Add(vertexColor);
        }
    }

    private static int[][] GetFillIndices(UIFillDirection fillDirection, int baseIndex) {
        var array = fillDirection == UIFillDirection.Horizontal ? HorizontalFillVertexGroups : VerticalFillVertexGroups;

        for (var i = 0; i < 4; i++) {
            for (var j = 0; j < 4; j++) {
                ActiveFillVertexGroups[i][j] = baseIndex + array[i][j];
            }
        }

        return ActiveFillVertexGroups;
    }

    private static void DoFill(UIRenderData renderData, SpriteRenderData spriteRenderData) {
        var baseIndex = spriteRenderData.BaseIndex;
        var vertices = renderData.vertices;
        var uvs = renderData.uvs;
        var groups = GetFillIndices(spriteRenderData.FillDirection, baseIndex);

        var invert = spriteRenderData.InvertFill;
        var axis = spriteRenderData.FillDirection == UIFillDirection.Horizontal ? 0 : 1;

        if (invert) {
            for (var i = 0; i < groups.Length; i++)
                Array.Reverse(groups[i]);
        }

        var start = vertices[groups[0][invert ? 3 : 0]][axis];
        var end = vertices[groups[0][invert ? 0 : 3]][axis];
        var length = Mathf.Abs(end - start);
        var fillPos = invert ? (end - spriteRenderData.FillAmount * length) : (start + spriteRenderData.FillAmount * length);

        for (var g = 0; g < groups.Length; g++) {
            var group = groups[g];

            if (!invert) {
                for (var i = 3; i > 0; i--) {
                    var vPos = vertices[group[i]][axis];
                    if (vPos >= fillPos) {
                        var v = vertices[group[i]];
                        v[axis] = fillPos;
                        vertices[group[i]] = v;

                        var prevPos = vertices[group[i - 1]][axis];
                        if (prevPos <= fillPos) {
                            var t = (fillPos - prevPos) / (vPos - prevPos);
                            var uv = uvs[group[i]];
                            uv[axis] = Mathf.Lerp(uvs[group[i - 1]][axis], uvs[group[i]][axis], t);
                            uvs[group[i]] = uv;
                        }
                    }
                }
            }
            else {
                for (var i = 1; i < 4; i++) {
                    var vPos = vertices[group[i]][axis];
                    if (vPos <= fillPos) {
                        var v = vertices[group[i]];
                        v[axis] = fillPos;
                        vertices[group[i]] = v;

                        var prevPos = vertices[group[i - 1]][axis];
                        if (prevPos >= fillPos) {
                            var t = (fillPos - prevPos) / (vPos - prevPos);
                            var uv = uvs[group[i]];
                            uv[axis] = Mathf.Lerp(uvs[group[i - 1]][axis], uvs[group[i]][axis], t);
                            uvs[group[i]] = uv;
                        }
                    }
                }
            }
        }
    }
}