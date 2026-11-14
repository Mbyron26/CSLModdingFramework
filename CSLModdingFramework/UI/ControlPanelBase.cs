using ColossalFramework.UI;
using CSLModdingFramework.UI.Controls;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class ControlPanelBase : Container {
    public virtual Vector2 DefaultSize => new(600, 200);
    public virtual float Padding => 16;
    public virtual float SettingsSectionWidth => DefaultSize.x - 2 * Padding;
    public virtual float TitleBarHeight => 46;

    protected virtual Button AddCloseButton() {
        var button = AddUIComponent<Button>();
        button.size = new Vector2(30, 30);

        var bg = button.Background;
        bg.Atlas = DefaultAtlas.Instance;
        bg.Sprites.SetValues(string.Empty, DefaultAtlas.Circle, DefaultAtlas.Circle, DefaultAtlas.Circle, string.Empty);
        bg.Colors.SetValues(UIColorPalette.ClassicDarkPalette.SettingsSectionBackgroundHovered);
        bg.Colors.Pressed = UIColorPalette.ClassicDarkPalette.SettingsSectionBackgroundPressed;
        var fg = button.Foreground;
        fg.Sprites.SetValues(DefaultAtlas.XClose);
        fg.ScaleMode = SpriteScaleMode.ContainOverflow;
        fg.Atlas = DefaultAtlas.Instance;
        fg.Scale = 0.6f;

        button.relativePosition = new Vector3(width - Padding - button.width, (TitleBarHeight - button.height) / 2);
        button.eventClicked += OnCloseButtonClicked;
        return button;
    }

    protected virtual DragArea AddDragArea(float dragAreaWidth) {
        var dragArea = AddUIComponent<DragArea>();
        dragArea.size = new Vector2(dragAreaWidth, TitleBarHeight);
        dragArea.relativePosition = Vector3.zero;
        return dragArea;
    }

    protected virtual SettingsSection AddSection(LiteContainer container, string header = null, string description = null) {
        var section = container.AddUIComponent<SettingsSection>();
        section.SuspendAutoLayout();
        section.width = SettingsSectionWidth;
        section.Theme = ThemeKind.ClassicDark;
        section.DescriptionScale = 0.8f;
        section.ItemLayoutPadding = new EdgeInsets(16, 16, 8, 8);

        if (string.IsNullOrEmpty(header)) {
            section.ResumeAutoLayout();
            return section;
        }

        section.Header = header;
        if (string.IsNullOrEmpty(description)) {
            section.ResumeAutoLayout();
            return section;
        }

        section.Description = description;
        section.ResumeAutoLayout();
        return section;
    }

    protected virtual void OnAwake() {
        Background.Atlas = DefaultAtlas.Instance;
        Background.Atlas = DefaultAtlas.Instance;
        Background.SpriteName = DefaultAtlas.CustomBackground;
        Background.NormalColor = UIColorPalette.ClassicDarkPalette.RootBackground;
        // Foreground.Atlas = DefaultAtlas.Instance;
        // Foreground.Sprite = DefaultAtlas.SmoothRectangleRadius12Outline2;
        // Foreground.ScaleMode = SpriteScaleMode.Stretch;
        // Foreground.NormalColor = UIColorPalette.ClassicDarkPalette.RootForeground;
    }

    protected virtual void OnCloseButtonClicked(UIComponent component, UIMouseEventParameter eventParam) { }

    public override void Awake() {
        base.Awake();

        m_CanFocus = true;
        m_IsInteractive = true;
        m_Size = DefaultSize;
        OnAwake();
    }
}