using System.Collections.Generic;
using System.Linq;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// ListViewのアイテム削除を元に戻すためのUndoRecord
    /// - 削除されたアイテムのElement以下のFieldBaseElementの値を記録する
    /// - Undoでアイテムを元に戻し、記録した値を復元する
    ///  - 復元時はElementの構造が変わっていない想定。DynamicElementなどで構造が変わっていると正しく復元できない可能性がある
    /// </summary>
    public class UndoRecordListItemRemove : UndoRecordListItemBase<UndoRecordListItemRemove>
    {
        public static void Register(ListViewItemContainerElement listElement, IEnumerable<int> indices)
        {
            if (!UndoHistory.CanAdd) return;
            
            var record = GetPooled();
            record.Initialize(listElement, indices);
            UndoHistory.Add(record);
        }
        
        
        public override string Name => "List Item Remove";
        
        private void Initialize(ListViewItemContainerElement listElement, IEnumerable<int> indices)
        {
            base.Initialize(listElement);

            ClearRecords();
            _records.AddRange(listElement.CreateRestoreRecords(indices));
        }
        
        // Undoで削除されたアイテムを元に戻し、値を復元する
        public override void Undo()
        {
            Element.GetListEditor().ApplyRestoreRecords(_records);
        }

        public override void Redo()
        {
            Element.GetListEditor().RemoveItems(_records.Select(r => r.index));
        }
    }
}