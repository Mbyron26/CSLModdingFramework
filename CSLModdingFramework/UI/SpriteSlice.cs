namespace CSLModdingFramework.UI;

public readonly struct SpriteSlice {
    public static readonly SpriteSlice None = new();

    public readonly int Left;
    public readonly int Right;
    public readonly int Top;
    public readonly int Bottom;

    public bool IsValid => Left > 0 || Right > 0 || Top > 0 || Bottom > 0;

    public SpriteSlice(int left, int right, int top, int bottom) {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    public SpriteSlice(int uniform) : this(uniform, uniform, uniform, uniform) { }
}