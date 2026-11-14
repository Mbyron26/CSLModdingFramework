#region

using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

#endregion

namespace CSLModdingFramework.UI;

public abstract class UIControl : UIComponent {
    public const string ExcludedControl = "__ExcludedControl";
    protected readonly Vector3[] _corners = new Vector3[4];

    public bool IsSizeChangedEventEnabled { get; set; } = true;
    public float UnitsPerPixel => m_CachedPixelSize > float.Epsilon ? m_CachedPixelSize : m_CachedPixelSize = GetUIView()?.PixelsToUnits() ?? 0.0026f;
    public PoolList<UIRenderData> RenderData => m_RenderData;

    public override void PerformLayout() { }
    public override void ResetLayout() { }

    protected override void OnSizeChanged() {
        if (IsSizeChangedEventEnabled)
            base.OnSizeChanged();
        else
            Invalidate();
    }

    public void DisableSizeChangedEvent() => IsSizeChangedEventEnabled = false;

    public void EnableSizeChangedEvent() => IsSizeChangedEventEnabled = true;

    public List<UIComponent> GetProcessableChildren() => m_ChildComponents.Where(child => child.isVisible && child.gameObject.activeSelf && child.name != ExcludedControl).ToList();

    public bool SetWidthIfChanged(float newWidth, bool notify = true, float epsilon = 0.005f) => SetSizeInternal(new Vector2(newWidth, m_Size.y), notify, epsilon);

    public bool SetHeightIfChanged(float newHeight, bool notify = true, float epsilon = 0.005f) => SetSizeInternal(new Vector2(m_Size.x, newHeight), notify, epsilon);

    public bool SetSizeIfChanged(Vector2 newSize, bool notify = true, float epsilon = 0.005f) => SetSizeInternal(newSize, notify, epsilon);

    public float PixelToUnit(float pixel) => pixel * UnitsPerPixel;

    public float UnitToPixel(float unit) => unit / UnitsPerPixel;

    public Vector3[] GetScreenPaddedCorners(in EdgeInsets padding) {
        var cam = UIView.GetAView()?.uiCamera;
        if (cam == null) return null;

        var world = GetPaddedCorners(padding);
        var screen = new Vector3[4];

        for (var i = 0; i < 4; i++) screen[i] = cam.WorldToScreenPoint(world[i]);

        return screen;
    }

    public Rect? GetScreenPaddedRect(in EdgeInsets padding) {
        GetPaddedCorners(padding);

        var minX = Mathf.Min(_corners[0].x, _corners[1].x, _corners[2].x, _corners[3].x);
        var maxX = Mathf.Max(_corners[0].x, _corners[1].x, _corners[2].x, _corners[3].x);
        var minY = Mathf.Min(_corners[0].y, _corners[1].y, _corners[2].y, _corners[3].y);
        var maxY = Mathf.Max(_corners[0].y, _corners[1].y, _corners[2].y, _corners[3].y);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    public Vector3[] GetPaddedCorners(EdgeInsets padding) {
        var units = UnitsPerPixel;
        var m = cachedTransform.localToWorldMatrix;

        var paddedWidth = size.x - padding.Left - padding.Right;
        var paddedHeight = size.y - padding.Top - padding.Bottom;

        var upperLeft = pivot.TransformToUpperLeft(new Vector2(paddedWidth, paddedHeight), arbitraryPivotOffset);

        upperLeft.x += padding.Left;
        upperLeft.y -= padding.Top;

        var x0 = upperLeft.x;
        var y0 = upperLeft.y;
        var x1 = x0 + paddedWidth;
        var y1 = y0 - paddedHeight;

        _corners[0] = m.MultiplyPoint(new Vector3(x0, y0, 0) * units); // LT
        _corners[1] = m.MultiplyPoint(new Vector3(x1, y0, 0) * units); // RT
        _corners[2] = m.MultiplyPoint(new Vector3(x1, y1, 0) * units); // RB
        _corners[3] = m.MultiplyPoint(new Vector3(x0, y1, 0) * units); // LB

        return _corners;
    }

    protected Plane[] GetPaddedClippingPlanes(EdgeInsets padding) {
        var corners = GetPaddedCorners(padding);

        var rightNormal = transform.TransformDirection(Vector3.right);
        var leftNormal = transform.TransformDirection(Vector3.left);
        var topNormal = transform.TransformDirection(Vector3.up);
        var bottomNormal = transform.TransformDirection(Vector3.down);

        m_CachedClippingPlanes[0] = new Plane(rightNormal, corners[0]);
        m_CachedClippingPlanes[1] = new Plane(leftNormal, corners[1]);
        m_CachedClippingPlanes[2] = new Plane(topNormal, corners[2]);
        m_CachedClippingPlanes[3] = new Plane(bottomNormal, corners[0]);

        return m_CachedClippingPlanes;
    }

    protected bool SetSizeInternal(Vector2 value, bool notify = true, float epsilon = 0.005f) {
        value = Vector2.Max(CalculateMinimumSize(), value);

        if (m_MaxSize.x > 0f) value.x = Mathf.Min(value.x, m_MaxSize.x);
        if (m_MaxSize.y > 0f) value.y = Mathf.Min(value.y, m_MaxSize.y);

        if (Mathf.Abs(value.x - m_Size.x) < epsilon &&
            Mathf.Abs(value.y - m_Size.y) < epsilon)
            return false;

        m_Size = value;
        if (notify)
            OnSizeChanged();
        else
            Invalidate();

        return true;
    }

    public static bool ContainsPoint(Vector2 point, Vector2 rectPosition, Vector2 rectSize)
        => point.x >= rectPosition.x &&
           point.x <= rectPosition.x + rectSize.x &&
           point.y >= rectPosition.y &&
           point.y <= rectPosition.y + rectSize.y;
}