using System.Collections.Generic;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// ListViewのアイテム削除を元に戻すためのUndoRecord
    /// - 削除されたアイテムのElement以下のFieldBaseElementの値を記録する
    /// - Undoでアイテムを元に戻し、記録した値を復元する
    ///  - 復元時はElementの構造が変わっていない想定。DynamicElementなどで構造が変わっていると正しく復元できない可能性がある
    /// </summary>
    public abstract class UndoRecordListItemBase<TRecord> : ElementUndoRecord<TRecord>
        where TRecord : UndoRecordListItemBase<TRecord>, new()
    {
        protected readonly List<ListViewItemContainerElement.RestoreRecord> records = new();
        
        protected ListViewItemContainerElement ListElement => (ListViewItemContainerElement)Element;
        
        
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
    
    public static class ListViewItemContainerElementExtensions
    {
        public static IEnumerable<ListViewItemContainerElement.RestoreRecord> CreateRestoreRecords(this ListViewItemContainerElement element, IEnumerable<int> indices)
        {
            var viewBridge = element.GetViewBridge();
            var list = viewBridge.GetIList();
            
            using var _ = Getter.CacheScope();
            foreach(var index in indices)
            {
                if (index < 0 || index >= list.Count)
                {
                    continue;
                }
                
                var isNull = list[index] == null;
                
                ElementRestoreRecord elementRecord = null;
                if (!isNull)
                {
                    var itemElement = viewBridge.GetOrCreateItemElement(index);
                    ElementRestoreRecord.TryCreate(itemElement, out elementRecord);
                }
                
                yield return new ListViewItemContainerElement.RestoreRecord(index, isNull, elementRecord);
            }
        }
    }
}