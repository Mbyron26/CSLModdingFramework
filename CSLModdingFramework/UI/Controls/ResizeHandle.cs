using System;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class ResizeHandle : UIInteractiveControl {
    protected IAtlas _atlas = DefaultAtlas.Instance;
    protected string _backgroundSpriteName = DefaultAtlas.ResizeHandle;
    protected ResizeEdge _edges = ResizeEdge.Right | ResizeEdge.Bottom;
    protected ColorStateValue Colors { get; private set; }
    private Vector3 _mouseAnchorPos;
    private Vector3 _startPosition;
    private Vector2 _startSize;
    private Vector2 _minEdgePos;
    private Vector2 _maxEdgePos;

    public IAtlas Atlas {
        get => _atlas;
        set {
            if (ReferenceEquals(value, Atlas)) return;

            _atlas = value;
            Invalidate();
        }
    }

    public string BackgroundSpriteName {
        get => _backgroundSpriteName;
        set {
            if (string.Equals(value, _backgroundSpriteName)) return;

            _backgroundSpriteName = value;
            Invalidate();
        }
    }

    public ResizeEdge Edges {
        get => _edges;
        set => _edges = value;
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(16f, 16f);
        Colors = new ColorStateValue(this);
    }

    protected override void OnRebuildRenderData() {
        if (_atlas == null) return;

        renderData.material = _atlas.Material;

        if (!_atlas.TryGetSprite(_backgroundSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = Colors.GetValue(_state),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = pivot.GetUpperLeftOffset(size, arbitraryPivotOffset),
            UnitsPerPixel = UnitsPerPixel,
            Size = size,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }

    protected override void OnMouseDown(UIMouseEventParameter p) {
        p.Use();
        var plane = new Plane(parent.transform.TransformDirection(Vector3.back), parent.transform.position);
        var ray = p.ray;
        plane.Raycast(ray, out var num);
        _mouseAnchorPos = ray.origin + ray.direction * num;
        _startSize = parent.size;
        _startPosition = parent.relativePosition;
        _minEdgePos = _startPosition;
        _maxEdgePos = _startPosition + (Vector3)_startSize;
        var vector = parent.CalculateMinimumSize();
        var vector2 = parent.maximumSize;
        if (vector2.magnitude <= 1E-45f) {
            vector2 = Vector2.one * 2048f;
        }

        if (_edges.IsFlagSet(ResizeEdge.Left)) {
            _minEdgePos.x = _maxEdgePos.x - vector2.x;
            _maxEdgePos.x = _maxEdgePos.x - vector.x;
        }
        else if (_edges.IsFlagSet(ResizeEdge.Right)) {
            _minEdgePos.x = _startPosition.x + vector.x;
            _maxEdgePos.x = _startPosition.x + vector2.x;
        }

        if (_edges.IsFlagSet(ResizeEdge.Top)) {
            _minEdgePos.y = _maxEdgePos.y - vector2.y;
            _maxEdgePos.y = _maxEdgePos.y - vector.y;
            return;
        }

        if (_edges.IsFlagSet(ResizeEdge.Bottom)) {
            _minEdgePos.y = _startPosition.y + vector.y;
            _maxEdgePos.y = _startPosition.y + vector2.y;
        }
    }

    protected override void OnMouseMove(UIMouseEventParameter p) {
        if (!p.buttons.IsFlagSet(UIMouseButton.Left) || Edges == ResizeEdge.None) {
            return;
        }

        p.Use();
        var ray = p.ray;
        var vector = GetCamera().transform.TransformDirection(Vector3.back);
        var plane = new Plane(vector, _mouseAnchorPos);
        plane.Raycast(ray, out var num);
        var num2 = PixelsToUnits();
        var vector2 = (ray.origin + ray.direction * num - _mouseAnchorPos) / num2;
        vector2.y *= -1f;
        var num3 = _startPosition.x;
        var num4 = _startPosition.y;
        var num5 = num3 + _startSize.x;
        var num6 = num4 + _startSize.y;
        if (Edges.IsFlagSet(ResizeEdge.Left)) {
            num3 = Mathf.Min(_maxEdgePos.x, Mathf.Max(_minEdgePos.x, num3 + vector2.x));
        }
        else if (Edges.IsFlagSet(ResizeEdge.Right)) {
            num5 = Mathf.Min(_maxEdgePos.x, Mathf.Max(_minEdgePos.x, num5 + vector2.x));
        }

        if (Edges.IsFlagSet(ResizeEdge.Top)) {
            num4 = Mathf.Min(_maxEdgePos.y, Mathf.Max(_minEdgePos.y, num4 + vector2.y));
        }
        else if (Edges.IsFlagSet(ResizeEdge.Bottom)) {
            num6 = Mathf.Min(_maxEdgePos.y, Mathf.Max(_minEdgePos.y, num6 + vector2.y));
        }

        parent.size = new Vector2(num5 - num3, num6 - num4).RoundToInt();
        parent.relativePosition = new Vector3(num3, num4, 0f).RoundToInt();
    }

    protected override void OnMouseUp(UIMouseEventParameter p) {
        parent.MakePixelPerfect();
        p.Use();
        base.OnMouseUp(p);
    }

    [Flags]
    public enum ResizeEdge {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8
    }
}