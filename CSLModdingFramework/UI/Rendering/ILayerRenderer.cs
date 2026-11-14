namespace CSLModdingFramework.UI.Rendering;

public interface ILayerRenderer<T> {
    T Owner { get; }
    void Render();
}