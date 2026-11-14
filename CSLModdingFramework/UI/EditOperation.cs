namespace CSLModdingFramework.UI;

public struct EditOperation {
    public EditOperationKind Kind;
    public int Index;
    public string Text;

    public int CursorBefore;
    public int AnchorBefore;

    public int CursorAfter;
    public int AnchorAfter;
}