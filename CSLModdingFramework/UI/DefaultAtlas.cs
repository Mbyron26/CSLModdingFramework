using System.Reflection;

namespace CSLModdingFramework.UI;

public sealed class DefaultAtlas : TextureAtlas {
    private static DefaultAtlas _instance;

    public DefaultAtlas() { }

    internal DefaultAtlas(string name) : base(name) => AtlasName = name;
    public static string Checked => nameof(Checked);

    public static string Circle => nameof(Circle);
    public static string Clean => nameof(Clean);

    public static string Copy => nameof(Copy);
    public static string Correct => nameof(Correct);

    public static string CustomBackground => nameof(CustomBackground);
    public static string DownArrow => nameof(DownArrow);
    public static string EmptySprite => nameof(EmptySprite);

    public static string Error => nameof(Error);
    public static string GradientSlider => nameof(GradientSlider);
    public static string Incorrect => nameof(Incorrect);
    public static string IndeterminateIcon => nameof(IndeterminateIcon);
    public static string ResizeHandle => nameof(ResizeHandle);

    public static DefaultAtlas Instance => _instance ??= Create<DefaultAtlas>($"{nameof(CSLModdingFramework)}{nameof(DefaultAtlas)}");
    public static string LeftArrow => nameof(LeftArrow);
    public static string LineBottom => nameof(LineBottom);
    public static string Loading => nameof(Loading);
    public static string Paste => nameof(Paste);
    public static string RadioButtonOn => nameof(RadioButtonOn);
    public static string RadioIndicator => nameof(RadioIndicator);
    public static string Rectangle => nameof(Rectangle);
    public static string RectangleBorder => nameof(RectangleBorder);

    public static string ResetOutline => nameof(ResetOutline);
    public static string RightArrow => nameof(RightArrow);
    public static string ToggleFgOff => nameof(ToggleFgOff);
    public static string ToggleFgOn => nameof(ToggleFgOn);

    public static string SmallRectangle => nameof(SmallRectangle);
    public static string SmoothRectangleRadius10 => nameof(SmoothRectangleRadius10);
    public static string SmoothRectangleRadius10Outline1 => nameof(SmoothRectangleRadius10Outline1);
    public static string SmoothRectangleRadius10Outline2 => nameof(SmoothRectangleRadius10Outline2);
    public static string SmoothRectangleRadius12 => nameof(SmoothRectangleRadius12);
    public static string SmoothRectangleRadius12Outline1 => nameof(SmoothRectangleRadius12Outline1);
    public static string SmoothRectangleRadius12Outline2 => nameof(SmoothRectangleRadius12Outline2);

    public static string SmoothRectangleRadius4 => nameof(SmoothRectangleRadius4);

    public static string SmoothRectangleRadius4Outline1 => nameof(SmoothRectangleRadius4Outline1);

    public static string SmoothRectangleRadius4Outline2 => nameof(SmoothRectangleRadius4Outline2);
    public static string SmoothRectangleRadius6 => nameof(SmoothRectangleRadius6);
    public static string SmoothRectangleRadius6Outline1 => nameof(SmoothRectangleRadius6Outline1);
    public static string SmoothRectangleRadius6Outline2 => nameof(SmoothRectangleRadius6Outline2);
    public static string SmoothRectangleRadius8 => nameof(SmoothRectangleRadius8);
    public static string SmoothRectangleRadius8Outline1 => nameof(SmoothRectangleRadius8Outline1);
    public static string SmoothRectangleRadius8Outline2 => nameof(SmoothRectangleRadius8Outline2);

    public static string ToggleBackground => nameof(ToggleBackground);

    public static string ToggleBg => nameof(ToggleBg);
    public static string ToggleForegroundOff => nameof(ToggleForegroundOff);
    public static string ToggleForegroundOn => nameof(ToggleForegroundOn);
    public static string ToggleOffFg => nameof(ToggleOffFg);
    public static string ToggleOnFg => nameof(ToggleOnFg);
    public static string TransparencySprite => nameof(TransparencySprite);
    public static string UpArrow => nameof(UpArrow);
    public static string Warn => nameof(Warn);
    public static string Warning => nameof(Warning);

    public static string XClose => nameof(XClose);

    public override string ResourcePath => $"{Assembly.GetExecutingAssembly().GetName().Name}.UI.Resources";

    protected override void RegisterSprites() {
        base.RegisterSprites();
        AddInternalSprite(CustomBackground, new SpriteSlice(12));
        AddInternalSprite(SmoothRectangleRadius4, new SpriteSlice(4));
        AddInternalSprite(SmoothRectangleRadius6, new SpriteSlice(6));
        AddInternalSprite(SmoothRectangleRadius8, new SpriteSlice(8));
        AddInternalSprite(SmoothRectangleRadius10, new SpriteSlice(10));
        AddInternalSprite(SmoothRectangleRadius12, new SpriteSlice(12));

        AddInternalSprite(SmoothRectangleRadius4Outline1, new SpriteSlice(4));
        AddInternalSprite(SmoothRectangleRadius6Outline1, new SpriteSlice(6));
        AddInternalSprite(SmoothRectangleRadius8Outline1, new SpriteSlice(8));
        AddInternalSprite(SmoothRectangleRadius10Outline1, new SpriteSlice(10));
        AddInternalSprite(SmoothRectangleRadius12Outline1, new SpriteSlice(12));
        AddInternalSprite(SmoothRectangleRadius4Outline2, new SpriteSlice(4));
        AddInternalSprite(SmoothRectangleRadius6Outline2, new SpriteSlice(6));
        AddInternalSprite(SmoothRectangleRadius8Outline2, new SpriteSlice(8));
        AddInternalSprite(SmoothRectangleRadius10Outline2, new SpriteSlice(10));
        AddInternalSprite(SmoothRectangleRadius12Outline2, new SpriteSlice(12));
        AddInternalSprite(SmallRectangle, new SpriteSlice(1, 1, 0, 0));

        AddInternalSprite(XClose, new SpriteSlice());
        AddInternalSprite(Rectangle, new SpriteSlice());
        AddInternalSprite(Circle, new SpriteSlice());
        AddInternalSprite(ToggleOnFg, new SpriteSlice());
        AddInternalSprite(ToggleOffFg, new SpriteSlice(12));

        AddInternalSprite(ToggleFgOn, new SpriteSlice());
        AddInternalSprite(ToggleFgOff, new SpriteSlice());

        AddInternalSprite(LineBottom, new SpriteSlice(1, 1, 0, 0));
        AddInternalSprite(GradientSlider, new SpriteSlice(8));
        AddInternalSprite(EmptySprite, new SpriteSlice(1, 1, 0, 0));
        AddInternalSprite(TransparencySprite, new SpriteSlice());

        AddInternalSprite(Copy, new SpriteSlice());
        AddInternalSprite(Paste, new SpriteSlice());
        AddInternalSprite(Clean, new SpriteSlice());

        AddInternalSprite(ToggleBg, new SpriteSlice());
        AddInternalSprite(ResetOutline, new SpriteSlice());
        AddInternalSprite(Checked, new SpriteSlice());
        AddInternalSprite(IndeterminateIcon, new SpriteSlice());
        AddInternalSprite(RadioIndicator, new SpriteSlice());
        AddInternalSprite(DownArrow, new SpriteSlice());
        AddInternalSprite(UpArrow, new SpriteSlice());
        AddInternalSprite(LeftArrow, new SpriteSlice());
        AddInternalSprite(RightArrow, new SpriteSlice());
        AddInternalSprite(Loading, new SpriteSlice());
        AddInternalSprite(Error, new SpriteSlice());
        AddInternalSprite(Warning, new SpriteSlice());
        AddInternalSprite(Warn, new SpriteSlice());
        AddInternalSprite(Correct, new SpriteSlice());
        AddInternalSprite(Incorrect, new SpriteSlice());
        AddInternalSprite(RadioButtonOn, new SpriteSlice());
        AddInternalSprite(ToggleBackground, new SpriteSlice());
        AddInternalSprite(ToggleForegroundOff, new SpriteSlice());
        AddInternalSprite(ToggleForegroundOn, new SpriteSlice());
        AddInternalSprite(RectangleBorder, new SpriteSlice(4));
        AddInternalSprite(ResizeHandle, new SpriteSlice());
    }
}