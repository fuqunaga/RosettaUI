using System;
using RosettaUI.Utilities;

namespace RosettaUI.UndoSystem
{
    /*
    /// <summary>
    /// ListViewItemContainer にバインドされているリストと状態を記録し復元するUndo機能向けのレコード
    /// UndoRecordListItemRemoveで利用される
    /// </summary>
    public class ListViewItemContainerElementRestoreRecord<TValue> : ObjectPoolItem<FieldBaseElementRestoreRecord<TValue>>, IElementRestoreRecord
    {
        public static IElementRestoreRecord Create(Element element)
        {
            if (element is not ListViewItemContainerElement listViewItemContainerElement)
            {
                throw new ArgumentException("element must be ListViewItemContainerElement");
            }
            
            var record = GetPooled();
            record.Initialize(listViewItemContainerElement);
            return record;
        }


        private Type _listType;
        private int? _itemCount;
    
        private void Initialize(ListViewItemContainerElement  listViewItemContainerElement)
        {
            var viewBridge = listViewItemContainerElement.GetViewBridge();
            _listType = viewBridge.GetListType();
            _itemCount = viewBridge.GetIList()?.Count;
        }
    
        public bool TryRestore(Element element)
        {
            if (element is not ListViewItemContainerElement listViewItemContainerElement) return false;
            
            Restore(listViewItemContainerElement);
            return true;
        }
    
        private void Restore(ListViewItemContainerElement listViewItemContainerElement)
        {
            
        }
    }
    */
}