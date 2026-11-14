namespace CSLModdingFramework.UI.Rendering;

public interface IAutoRender {
    bool IsPerformingAutoRender { get; }
    bool IsAutoRenderSuspended { get; }
    void SuspendAutoRender();
    void ResumeAutoRender(bool performNow = true);
    void PerformAutoRender();
}