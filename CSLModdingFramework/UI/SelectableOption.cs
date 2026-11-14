namespace CSLModdingFramework.UI;

public class SelectableOption<T> : SelectableOptionBase<T> {
    public SelectableOption(T value) : base(value) { }

    public SelectableOption(T value, string text, IAtlas iconAtlas, string iconName, bool isEnabled, bool isVisible) : base(value, isEnabled, isVisible) {
        Text = text;
        IconAtlas = iconAtlas;
        IconName = iconName;
    }

    internal SelectableOption(int id, T value) : base(id, value) { }

    public string Text { get; internal set; }
    public IAtlas IconAtlas { get; internal set; }
    public string IconName { get; internal set; }
}