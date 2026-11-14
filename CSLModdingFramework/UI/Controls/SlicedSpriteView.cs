using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;

namespace CSLModdingFramework.UI.Controls;

public class SlicedSpriteView : SpriteView {
    protected override void OnRebuildRenderData() {
        if (_atlas == null || _atlas.Material == null || !isVisible) return;

        renderData.material = _atlas.Material;

        if (string.IsNullOrEmpty(_spriteName)) return;

        if (!_atlas.TryGetSprite(_spriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

        if (spriteMetadata.Region.height == 0 || spriteMetadata.Region.width == 0) {
            base.OnRebuildRenderData();
            return;
        }

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = Colors.GetValue(_state),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = GetRenderOffset(),
            UnitsPerPixel = UnitsPerPixel,
            Size = size,
            SpriteMetadata = spriteMetadata
        };

        SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }
}