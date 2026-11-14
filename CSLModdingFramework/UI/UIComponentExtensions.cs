using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CSLModdingFramework.UI;

public static class UIComponentExtensions {
    public static Rect? GetScreenRect(this UIComponent comp) {
        var c = comp.GetScreenCorners();
        if (c == null)
            return null;

        var minX = Mathf.Min(c[0].x, c[1].x, c[2].x, c[3].x);
        var maxX = Mathf.Max(c[0].x, c[1].x, c[2].x, c[3].x);
        var minY = Mathf.Min(c[0].y, c[1].y, c[2].y, c[3].y);
        var maxY = Mathf.Max(c[0].y, c[1].y, c[2].y, c[3].y);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    public static Vector3[] GetScreenCorners(this UIComponent comp) {
        if (comp == null) return null;

        var cam = UIView.GetAView()?.uiCamera;
        if (cam == null)
            return null;
        var world = comp.GetCorners();
        var screen = new Vector3[4];

        for (var i = 0; i < 4; i++) {
            screen[i] = cam.WorldToScreenPoint(world[i]);
        }

        return screen;
    }

    public static T AddUIComponent<T>(this UIComponent parent, Action<T> configure) where T : UIComponent {
        var component = parent.AddUIComponent<T>();
        configure?.Invoke(component);
        return component;
    }

    public static void DestroyChildrenAndSelf(this UIComponent component) {
        if (component == null) return;
        component.DestroyAllChildren();
        component.DestroySelf();
    }

    public static void DestroySelf(this UIComponent component) {
        if (component == null) return;
        if (component.parent != null) component.RemoveUIComponent(component);

        if (component.gameObject != null) Object.Destroy(component.gameObject);
    }

    public static void DestroyAllChildren(this UIComponent component) {
        if (component == null || component.parent == null) return;
        var children = new List<UIComponent>(component.components);
        foreach (var child in children) child.DestroySelf();
    }
}