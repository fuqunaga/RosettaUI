using System.Collections.Generic;
using RosettaUI.Utilities;
using UnityEngine.Pool;

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
        private static IEnumerable<Element> FlattenElements(Element targetElement)
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
                    foreach (var e in FlattenElements(child))
                    {
                        yield return e;
                    }
                }
            }
            else
            {
                yield return targetElement;
            }
        }
        
        public static IEnumerable<IElementRestoreRecord> CreateRecords(Element targetElement)
        {
            foreach (var e in FlattenElements(targetElement))
            {
                if (FieldBaseElementRestoreRecord.TryCreate(e, out var record))
                {
                    yield return record;
                }
            }
        
        }
        
        // CreateElementRecordsで作成したレコードを元にElementの状態を復元する
        // Elementの構造が変わっていない想定で順番にrecordsを適用していく
        public static void Restore(Element targetElement, IEnumerable<IElementRestoreRecord> records)
        {
            using var recordsEnumerator = records.GetEnumerator();
            recordsEnumerator.MoveNext();
            
            foreach (var e in FlattenElements(targetElement))
            {
                if (recordsEnumerator.Current == null)
                {
                    break;
                }
                
                if ( recordsEnumerator.Current.TryRestore(e) )
                {
                    recordsEnumerator.MoveNext();
                }
            }
        }
        
        #endregion

        
        private List<IElementRestoreRecord> _records;
        
        private bool Initialize(Element element)
        {
            ListPool<IElementRestoreRecord>.Get(out _records);
            _records.AddRange(CreateRecords(element));
            return _records.Count > 0;
        }

        public override void Dispose()
        {
            foreach (var record in _records)
            {
                record.Dispose();
            }
            ListPool<IElementRestoreRecord>.Release(_records);
            _records = null;
            
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