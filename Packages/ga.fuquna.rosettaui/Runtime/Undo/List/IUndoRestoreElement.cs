namespace RosettaUI.Undo
{
    /// <summary>
    /// 削除されたあとUndoで復元可能なElementに実装するインターフェース
    /// - リストで要素された際のUndoとして機能する
    /// </summary>
    public interface IUndoRestoreElement
    {
        IElementRestoreRecord CreateRestoreRecord();
    }
}