using System;

namespace RosettaUI.Undo
{
    /// <summary>
    /// UndoHistoryで管理されるUndoの記録
    /// - Undo/Redoの処理を持つ
    /// - Dispose可能（UndoHistoryから削除されたときにDisposeされる）
    /// - マージ可能（UndoHistory上最新のIUndoRecordとCanMargeなら一つにまとめる。同じElementへの連続した変更など）
    /// </summary>
    public interface IUndoRecord : IDisposable
    {
        string Name { get; }
        bool IsExpired { get; }
        
        void Undo();
        void Redo();
        
        bool CanMerge(IUndoRecord newer);
        void Merge(IUndoRecord newer);
    }
}