using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class ScrollFlexContainer : ScrollContainerBase<FlexContainer> {
    protected override void CreateContentContainer() {
        _contentContainer = AddUIComponent<FlexContainer>();
        _contentContainer.size = size;
        _contentContainer.relativePosition = Vector3.zero;
        _contentContainer.AutoFitChildrenVertically = true;
        UpdateContentConstraints();
        _contentContainer.eventSizeChanged += OnContentContainerSizeChanged;
    }
}