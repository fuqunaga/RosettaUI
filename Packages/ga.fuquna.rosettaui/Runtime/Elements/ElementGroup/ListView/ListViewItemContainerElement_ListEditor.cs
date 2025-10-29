using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RosettaUI.UndoSystem;
using UnityEngine.Pool;

namespace RosettaUI
{
    public partial class ListViewItemContainerElement : IUndoRestoreElement
    {
        #region IUndoRedoElement

        public IElementRestoreRecord CreateRestoreRecord() => ListViewItemContainerElementRestoreRecord.Create(this);

        #endregion
        
        
        /// <summary>
        /// RosettaUI側でListを編集するためのインターフェースを取得 
        /// </summary>
        public ListEditor GetListEditor() => new(this);
        
        /// <summary>
        /// ライブラリ側でListViewのListを編集するためのインターフェースクラス
        /// 
        /// Elementの値の編集は通常、アプリ側とUI側の２箇所でBinder経由でやりとりするが、
        /// Undoや別のUIからの操作のようなライブラリ側の編集はこれらとは異なりどちらに対しても通知する必要がある
        /// - UIからの変更扱いにするとアプリは追従できるがUIが追従しない
        /// - アプリからの変更扱いはそもそもどう変更されたか不明なためUIを作り直してしまう（Foldやスクロールの状態を引き継げない）
        /// UIの状態を引き継ぎ、かつ、アプリもUIも追従させるための編集インターフェース
        /// </summary>
        public readonly struct ListEditor
        {
            private ListViewItemContainerElement Element { get; }
            private ListBinder ListBinder => Element.ListBinder;
            public IList CurrentList => ListBinder.GetIList();
            
            
            public ListEditor(ListViewItemContainerElement element) => Element = element;

            
            private void NotifyListChanged()
            {
                Element.NotifyListChangedToView();
                Element.NotifyViewValueChanged();
            }

            public void AddItem(int index)
            {
                ListBinder.AddItem(index);
                Element.OnItemIndexShiftPlus(index + 1);
                
                Undo.RecordListItemAdd(Element, index + 1);
                
                NotifyListChanged();
            }
            
            public void RemoveItem(int index)
            {
                using var _ = ListPool<int>.Get(out var indices);
                indices.Add(index);
                RemoveItems(indices);
            }
            
            [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
            public void RemoveItems(IEnumerable<int> indices)
            {
                Undo.RecordListItemRemove(Element, indices);
                
                foreach (var index in indices.OrderByDescending(i => i))
                {
                    ListBinder.RemoveItem(index);
                    Element.OnItemIndexShiftMinus(index);
                }
                
                NotifyListChanged();
            }
            
            public void MoveItem(int fromIndex, int toIndex)
            {
                if (fromIndex == toIndex) return;
                
                ListBinder.MoveItem(fromIndex, toIndex);
                Element.OnMoveItemIndex(fromIndex, toIndex);
                
                Undo.RecordListItemMove(Element, fromIndex, toIndex);
                
                NotifyListChanged();
            }
            
            
            public IEnumerable<RestoreRecord> CreateRestoreRecordsForAllItems() => CreateRestoreRecords(Enumerable.Range(0, CurrentList?.Count ?? 0));

            public void ApplyRestoreRecordsForAllItems(IEnumerable<RestoreRecord> records)
            {
                var list = CurrentList;
                
                // いったん全削除
                var count = list?.Count ?? 0;
                if (count > 0)
                {
                    ListBinder.RemoveItems(..count);
                }
                
                ApplyRestoreRecords(records);
            }
            
            public IEnumerable<RestoreRecord> CreateRestoreRecords(IEnumerable<int> indices)
            {
                var list = CurrentList;
                if (list == null)
                {
                    yield break;
                }
            
                using var _ = Getter.CacheScope();
                foreach(var index in indices)
                {
                    if (index < 0 || index >= list.Count)
                    {
                        continue;
                    }
                
                    var item = list[index];
                    var isNull = item == null;
                
                    ElementRestoreRecord elementRecord = null;
                    ElementState elementState = null;
                    IObjectRestoreRecord objectRestoreRecord = null;
                    if (!isNull)
                    {
                        var itemElement = Element.GetOrCreateItemElement(index);
                        ElementRestoreRecord.TryCreate(itemElement, out elementRecord);
                        elementState = ElementState.Create(itemElement);
                        objectRestoreRecord = Element.option.createItemRestoreRecordFunc?.Invoke(item);
                    }
                
                    yield return new RestoreRecord(index, isNull, elementRecord, elementState, objectRestoreRecord);
                }
            }

            public void ApplyRestoreRecords(IEnumerable<RestoreRecord> records)
            {
                foreach (var record in records.OrderBy(r => r.index))
                {
                    var index = record.index;
                    if (record.isNull)
                    {
                        ListBinder.AddNullItem(index);
                    }
                    else
                    {
                        if (record.objectRestoreRecord != null)
                        {
                            var restoredObject = record.objectRestoreRecord.RestoreObject();
                            ListBinder.AddItem(restoredObject, index);
                        }
                        else
                        {
                            ListBinder.AddItem(index);
                        }
                    }

                    Element.OnItemIndexShiftPlus(index);
                    
                    var elementRecord = record.record;
                    if (elementRecord != null)
                    {
                        var itemElement = Element.GetOrCreateItemElement(index);
                        elementRecord.Restore(itemElement);
                    }
                    
                    var state = record.state;
                    if (state != null)
                    {
                        var itemElement = Element.GetOrCreateItemElement(index);
                        state.Apply(itemElement);
                    }
                }
                
                NotifyListChanged();
            }
        }
        
        /// <summary>
        /// 削除されたアイテムをUndoで元に戻すためのRecord 
        /// </summary>
        public readonly struct RestoreRecord : IDisposable
        {
            public readonly int index;
            public readonly bool isNull;
            public readonly ElementRestoreRecord record;
            public readonly ElementState state;
            public readonly IObjectRestoreRecord objectRestoreRecord;

            public RestoreRecord(int index, bool isNull, ElementRestoreRecord record, ElementState state, IObjectRestoreRecord objectRestoreRecord)
            {
                this.index = index;
                this.isNull = isNull;
                this.record = record;
                this.state = state;
                this.objectRestoreRecord = objectRestoreRecord;
            }

            public void Dispose()
            {
                record?.Dispose();
                state?.Dispose();
                
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (objectRestoreRecord is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}