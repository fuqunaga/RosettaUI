using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace RosettaUI.UndoSystem
{
    public class UndoRecordListItemRemove : ElementUndoRecord<UndoRecordListItemRemove>
    {
        private static readonly ObjectPool<List<IRestoreToElementRecord>> RecordListPool = new(
            () => new List<IRestoreToElementRecord>(),
            actionOnRelease: list =>
            {
                foreach (var item in list)
                {
                    item.Dispose();
                }
                list.Clear();
            });
        
        public static void Register(ListViewItemContainerElement listElement, IEnumerable<int> indices)
        {
            var record = GetPooled();
            record.Initialize(listElement, indices);
            UndoHistory.Add(record);
        }

        private readonly Dictionary<int, List<IRestoreToElementRecord>> _indexToRecord = new();
        
        
        private ListViewItemContainerElement Element => (ListViewItemContainerElement)element;
        
        public override string Name => "List Item Remove";

        
        private void Initialize(ListViewItemContainerElement listElement, IEnumerable<int> indices)
        {
            base.Initialize(listElement);

            ClearRecords();
            
            var viewBridge = listElement.GetViewBridge();
            
            using var _ = Getter.CacheScope();
            foreach(var index in indices)
            {
                var itemElement = viewBridge.GetOrCreateItemElement(index);
         
                // itemElement以下から現在の値を記録したUndoRecordを再帰的に作る
                // FieldBaseElementのほかにListViewが入れ子になっている場合もある
                // UndoableValueElementみたいなグループの定義が必要かも
                
                var records = CreateElementRecords(itemElement);
                var list = RecordListPool.Get();
                list.AddRange(records);

                if (list.Count == 0)
                {
                    RecordListPool.Release(list);
                    continue;
                }

                _indexToRecord[index] = list;
            }
        }
        
        private void ClearRecords()
        {
            foreach (var record in _indexToRecord.Values)
            {
                RecordListPool.Release(record);
            }
            _indexToRecord.Clear();
        }
        
        public override void Dispose()
        {
            ClearRecords();
            base.Dispose();
        }
        
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
        
        private static IEnumerable<IRestoreToElementRecord> CreateElementRecords(Element targetElement)
        {
            foreach (var e in FlattenElements(targetElement))
            {
                if (FieldBaseElementValueRecord.TryCreate(e, out var record))
                {
                    yield return record;
                }
            }
        
        }
        
        // CreateElementRecordsで作成したレコードを元にElementの状態を復元する
        // Elementの構造が変わっていない想定で順番にrecordsを適用していく
        private static void RestoreElementRecords(Element targetElement, IEnumerable<IRestoreToElementRecord> records)
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
        
        public override void Undo()
        {
            var viewBridge = Element.GetViewBridge();
            var list = viewBridge.GetIList();
            var itemType = ListUtility.GetItemType(list.GetType());

            using var _ = ListPool<int>.Get(out var indices);
            indices.AddRange(_indexToRecord.Keys.OrderBy(i => i));
            
            // Binder経由でのアプリケーションの通知を考慮し要素の追加をまとめて先に行う
            // １要素ごとに追加と値変更と行うと追加ごとに変更通知が行われるてしまう
            list = indices.Aggregate(list, (currentList, index) => ListUtility.AddItem(currentList, itemType, null, index));
            
            viewBridge.OnViewListChanged(list);
            
            foreach (var index in indices)
            {
                var records = _indexToRecord[index];
                
                var itemElement = viewBridge.GetOrCreateItemElement(index);
                RestoreElementRecords(itemElement, records);
            }
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