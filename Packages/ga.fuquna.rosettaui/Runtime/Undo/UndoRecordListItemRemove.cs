using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// ListViewのアイテム削除を元に戻すためのUndoRecord
    /// - 削除されたアイテムのElement以下のFieldBaseElementの値を記録する
    /// - Undoでアイテムを元に戻し、記録した値を復元する
    ///  - 復元時はElementの構造が変わっていない想定。DynamicElementなどで構造が変わっていると正しく復元できない可能性がある
    /// </summary>
    public class UndoRecordListItemRemove : ElementUndoRecord<UndoRecordListItemRemove>
    {
        public static void Register(ListViewItemContainerElement listElement, ReadOnlySpan<int> indicesOrderedDescending)
        {
            if (!UndoHistory.CanAdd) return;
            
            var record = GetPooled();
            record.Initialize(listElement, indicesOrderedDescending);
            UndoHistory.Add(record);
        }
        
        private readonly List<ListViewItemContainerElement.RestoreRecord> _records = new();
        
        
        private ListViewItemContainerElement Element => (ListViewItemContainerElement)element;
        
        public override string Name => "List Item Remove";

        
        private void Initialize(ListViewItemContainerElement listElement, ReadOnlySpan<int> indices)
        {
            base.Initialize(listElement);

            ClearRecords();
            
            var viewBridge = listElement.GetViewBridge();
            var list = viewBridge.GetIList();
            
            using var _ = Getter.CacheScope();
            foreach(var index in indices)
            {
                var isNull = list[index] == null;
                
                ElementRestoreRecord elementRecord = null;
                if (!isNull)
                {
                    var itemElement = viewBridge.GetOrCreateItemElement(index);
                    ElementRestoreRecord.TryCreate(itemElement, out elementRecord);
                }
                
                _records.Add(new ListViewItemContainerElement.RestoreRecord(index, isNull, elementRecord));
            }
        }
        
        private void ClearRecords()
        {
            foreach (var record in _records)
            {
                record.Dispose();
            }
            _records.Clear();
        }
        
        public override void Dispose()
        {
            ClearRecords();
            base.Dispose();
        }
    
        
        // Undoで削除されたアイテムを元に戻し、値を復元する
        public override void Undo()
        {
            Element.GetListEditor().ApplyRestoreRecords(_records);
        }

        public override void Redo()
        {
            Span<int> indices = stackalloc int[_records.Count];
            var spanIndex = 0;
            foreach (var index in _records.Select(r => r.index).OrderByDescending(i => i))
            {
                indices[spanIndex++] = index;
            }
            
            Element.GetListEditor().RemoveItems(indices);
        }

        public override bool CanMerge(IUndoRecord _) => false;
        
        public override void Merge(IUndoRecord _)
        {
        }
    }
}