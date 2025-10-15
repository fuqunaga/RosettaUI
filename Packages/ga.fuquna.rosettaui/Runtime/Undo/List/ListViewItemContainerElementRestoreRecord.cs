using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Utilities;

namespace RosettaUI.Undo
{
    /// <summary>
    /// Listの要素がListを含む入れ子の構造になっている場合の要素内のListをUndo時に復元するためのレコード
    /// </summary>
    public class ListViewItemContainerElementRestoreRecord : ObjectPoolItem<ListViewItemContainerElementRestoreRecord>, IElementRestoreRecord
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

        private readonly List<ListViewItemContainerElement.RestoreRecord> _restoreRecords = new();

        private void Initialize(ListViewItemContainerElement listViewItemContainerElement)
        {
            Clear();

            var listEditor = listViewItemContainerElement.GetListEditor();
            _restoreRecords.AddRange(listEditor.CreateRestoreRecordsForAllItems());
        }

        private void Clear()
        {
            _restoreRecords.Clear();
        }

        public bool TryRestore(IUndoRestoreElement element)
        {
            if (element is not ListViewItemContainerElement listViewItemContainerElement) return false;
            
            var listEditor = listViewItemContainerElement.GetListEditor();
            listEditor.ApplyRestoreRecordsForAllItems(_restoreRecords);
            
            return true;
        }
        
        public override void Dispose()
        {
            Clear();
            base.Dispose();
        }
    }
}