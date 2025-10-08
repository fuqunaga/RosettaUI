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

        private readonly Dictionary<int, ElementRestoreRecord> _indexToRecord = new();
        
        
        private ListViewItemContainerElement Element => (ListViewItemContainerElement)element;
        
        public override string Name => "List Item Remove";

        
        private void Initialize(ListViewItemContainerElement listElement, ReadOnlySpan<int> indices)
        {
            base.Initialize(listElement);

            ClearRecords();
            
            var viewBridge = listElement.GetViewBridge();
            
            using var _ = Getter.CacheScope();
            foreach(var index in indices)
            {
                var itemElement = viewBridge.GetOrCreateItemElement(index);
                if (ElementRestoreRecord.TryCreate(itemElement, out var record))
                {
                    _indexToRecord[index] = record;    
                }
            }
        }
        
        private void ClearRecords()
        {
            foreach (var record in _indexToRecord.Values)
            {
                record.Dispose();
            }
            _indexToRecord.Clear();
        }
        
        public override void Dispose()
        {
            ClearRecords();
            base.Dispose();
        }
    
        
        // Undoで削除されたアイテムを元に戻し、値を復元する
        public override void Undo()
        {
            Element.GetListEditor().ApplyRestoreRecords(_indexToRecord);
        }

        public override void Redo()
        {
            var viewBridge = Element.GetViewBridge();
            var list = viewBridge.GetIList();
            var itemType = ListUtility.GetItemType(list.GetType());

            using var _ = ListPool<int>.Get(out var indices);
            indices.AddRange(_indexToRecord.Keys.OrderByDescending(i => i));

            list = indices.Aggregate(list, (currentList, index) => ListUtility.RemoveItem(currentList, itemType, index));
            viewBridge.OnViewListChanged(list);
        }

        public override bool CanMerge(IUndoRecord _) => false;
        
        public override void Merge(IUndoRecord _)
        {
        }
    }
}