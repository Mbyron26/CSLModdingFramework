namespace CSLModdingFramework.UI;

public interface IAutoLayout {
    bool IsPerformingAutoLayout { get; }
    void SuspendAutoLayout();
    void ResumeAutoLayout(bool performNow = true);
    void PerformAutoLayout();
}

/*
public class AutoLayoutControl : UIInteractiveControl, IAutoLayout {
    protected int _autoLayoutSuspendCount;

    public bool IsPerformingAutoLayout { get; protected set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;

        IsPerformingAutoLayout = false;
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }
}
*/