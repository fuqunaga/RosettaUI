using System.Collections.Generic;

namespace RosettaUI.Undo
{
    /// <summary>
    /// ListViewのアイテム削除を元に戻すためのUndoRecord
    /// - 削除されたアイテムのElement以下のFieldBaseElementの値を記録する
    /// - Undoでアイテムを元に戻し、記録した値を復元する
    ///  - 復元時はElementの構造が変わっていない想定。DynamicElementなどで構造が変わっていると正しく復元できない可能性がある
    /// </summary>
    public abstract class UndoRecordListItemRestoreBase<TRecord> : UndoRecordElementBase<ListViewItemContainerElement>
        where TRecord : UndoRecordListItemRestoreBase<TRecord>, new()
    {
        protected readonly List<ListViewItemContainerElement.RestoreRecord> records = new();

        protected void ClearRecords()
        {
            foreach (var record in records)
            {
                record.Dispose();
            }

            records.Clear();
        }

        public override void Dispose()
        {
            ClearRecords();
            base.Dispose();
        }
    }
}