using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace RosettaUI.Undo
{
    /// <summary>
    /// ListViewのアイテム追加を元に戻すためのUndoRecord
    /// Redo時に削除されたアイテムを復元する必要があるためUndoRecordItemRemoveのように、Undo時削除するまえにの値を記録しておく必要がある
    /// </summary>
    public class UndoRecordListItemAdd : UndoRecordListItemRestoreBase<UndoRecordListItemAdd>
    {
        public static void Record(ListViewItemContainerElement listElement, int index)
        {
            using var _ = ListPool<int>.Get(out var indices);
            indices.Add(index);
            Record(listElement, indices);
        }
        
        public static void Record(ListViewItemContainerElement listElement, IEnumerable<int> indices)
        {
            if (!UndoHistory.CanAdd) return;
    
            using (UndoRecorder<UndoRecordListItemAdd>.Get(out var record))
            {
                record.Initialize(listElement, indices);
            }
        }
        
        
        public override string Name => $"List Item Add index:({string.Join(", ", records.Select(r => r.index))})";
        
        private void Initialize(ListViewItemContainerElement listElement, IEnumerable<int> indices)
        {
            base.Initialize(listElement);

            ClearRecords();
            
            // Undo時に必要なのはインデックスだけなのでインデックスのみのRestoreRecordを保存
            records.AddRange(indices.Select(i => new ListViewItemContainerElement.RestoreRecord(i, false, null, null)));
        }
        
        // 追加されたアイテムを削除する
        // Redo時に復元するため、削除するまえの値を記録しておく
        public override void Undo()
        {
            using var _ = ListPool<int>.Get(out var indices);
            indices.AddRange(records.Select(r => r.index));
            
            ClearRecords();
            records.AddRange(Element.GetListEditor().CreateRestoreRecords(indices));

            Element.GetListEditor().RemoveItems(indices);
        }

        public override void Redo()
        {
            Element.GetListEditor().ApplyRestoreRecords(records);
        }
    }
}