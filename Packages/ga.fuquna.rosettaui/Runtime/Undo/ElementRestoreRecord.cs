using System.Collections.Generic;
using System.Linq;
using RosettaUI.Utilities;
using UnityEngine.UIElements;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// Elementの状態を保存、復元する
    /// 内部的にElementの特定の型ごとのIElementRestoreRecordを複数持つ
    /// </summary>
    public class ElementRestoreRecord : ObjectPoolItem<ElementRestoreRecord>, IElementRestoreRecord
    {
        #region Static
        
        public static bool TryCreate(Element element, out ElementRestoreRecord record)
        {
            record = GetPooled();
            if (record.Initialize(element))
            {
                return true;
            }
            
            record.Dispose();
            record = null;
            return false;

        }

        // targetElement以下のElementを再帰的にフラットに列挙する
        // Recordを作成・復元する際に同じ構成なら同じ順番で列挙されることを期待している
        private static IEnumerable<IUndoRestoreElement> FlattenUndoRestoreElements(Element targetElement)
        {
            if (targetElement is ListViewItemContainerElement listElementContainer)
            {
                // todo
                yield break;
            }
                
            if (targetElement is ElementGroup group)
            {
                foreach (var child in group.Contents)
                {
                    foreach (var e in FlattenUndoRestoreElements(child))
                    {
                        yield return e;
                    }
                }
            }
            else if(targetElement is IUndoRestoreElement undoRestoreElement)
            {
                yield return undoRestoreElement;
            }
        }
        
        public static IEnumerable<IElementRestoreRecord> CreateRecords(Element targetElement)
        {
            return FlattenUndoRestoreElements(targetElement).Select(e => e.CreateRestoreRecord());
        }
        
        // CreateElementRecordsで作成したレコードを元にElementの状態を復元する
        // Elementの構造が変わっていない想定で順番にrecordsを適用していく
        public static void Restore(Element targetElement, IEnumerable<IElementRestoreRecord> records)
        {
            using var recordsEnumerator = records.GetEnumerator();
            recordsEnumerator.MoveNext();
            
            foreach (var e in FlattenUndoRestoreElements(targetElement))
            {
                if (recordsEnumerator.Current == null)
                {
                    break;
                }
                
                if ( e.TryRestore(recordsEnumerator.Current) )
                {
                    recordsEnumerator.MoveNext();
                }
            }
        }
        
        #endregion


        private readonly List<IElementRestoreRecord> _records = new();
        
        private bool Initialize(Element element)
        {
            _records.Clear();
            _records.AddRange(CreateRecords(element));
            return _records.Count > 0;
        }

        public override void Dispose()
        {
            foreach (var record in _records)
            {
                record.Dispose();
            }
            _records.Clear();
            
            base.Dispose();
        }

        public bool TryRestore(Element element)
        {
            Restore(element);
            return true;
        }

        public void Restore(Element element)
        {
            Restore(element, _records);
        }
    }
}