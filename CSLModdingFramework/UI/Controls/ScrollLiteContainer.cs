using System;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

[Obsolete("Use ScrollViewer instead")]
public class ScrollLiteContainer : ScrollContainerBase<LiteContainer> {
    protected override void CreateContentContainer() {
        _contentContainer = AddUIComponent<LiteContainer>();
        _contentContainer.size = size;
        _contentContainer.relativePosition = Vector3.zero;
        _contentContainer.AutoFitChildrenVertically = true;
        UpdateContentConstraints();
        _contentContainer.eventSizeChanged += OnContentContainerSizeChanged;
    }
}