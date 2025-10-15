using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RosettaUI.UndoSystem;
using UnityEngine.Pool;

namespace RosettaUI
{
    public partial class ListViewItemContainerElement
    {
        /// <summary>
        /// ライブラリ側でListViewのListを編集するためのインターフェースクラス
        /// 
        /// Elementの値の編集は通常、アプリ側とUI側の２箇所でBinder経由でやりとりするが、
        /// Undoや別のUIからの操作のようなライブラリ側の編集はこれらとは異なりどちらに対しても通知する必要がある
        /// - UIからの変更扱いにするとアプリは追従できるがUIが追従しない
        /// - アプリからの変更扱いはそもそもどう変更されたか不明なためUIを作り直してしまう（Foldやスクロールの状態を引き継げない）
        /// UIの状態を引き継ぎ、かつ、アプリもUIも追従させるための編集インターフェース
        /// </summary>
        public struct ListEditor
        {
            public ListViewItemContainerElement Element { get; private set; }
            private IBinder Binder => Element._binder;
            
            public ListEditor(ListViewItemContainerElement element) => Element = element;

            private void NotifyListChanged()
            {
                Element.NotifyListChangedToView();
                Element.NotifyViewValueChanged();
            }

            public void DuplicateItem(int index)
            {
                UndoRecordListItemAdd.Register(Element, index);
                
                ListBinder.DuplicateItem(Binder, index);
                Element.OnItemIndexShiftPlus(index + 1);
                
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
                UndoRecordListItemRemove.Record(Element, indices);
                
                foreach (var index in indices.OrderByDescending(i => i))
                {
                    ListBinder.RemoveItem(Binder, index);
                    Element.OnItemIndexShiftMinus(index);
                }
                
                NotifyListChanged();
            }

            public void ApplyRestoreRecords(IEnumerable<RestoreRecord> records)
            {
                foreach (var record in records.OrderBy(r => r.index))
                {
                    var index = record.index;
                    if (record.isNull)
                    {
                        ListBinder.AddNullItem(Binder, index);
                    }
                    else
                    {
                        ListBinder.AddItem(Binder, index);
                    }

                    Element.OnItemIndexShiftPlus(index);
                    
                    var elementRecord = record.record;
                    if (elementRecord != null)
                    {
                        var itemElement = Element.GetOrCreateItemElement(index);
                        elementRecord.Restore(itemElement);
                    }
                }
                
                NotifyListChanged();
            }
            
            public IEnumerable<RestoreRecord> CreateRestoreRecords(IEnumerable<int> indices)
            {
                var list = ListBinder.GetIList(Binder);
            
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
                        var itemElement = Element.GetOrCreateItemElement(index);
                        ElementRestoreRecord.TryCreate(itemElement, out elementRecord);
                    }
                
                    yield return new RestoreRecord(index, isNull, elementRecord);
                }
            }
        }
        
        
        // 削除されたアイテムをUndoで元に戻すためのRecord
        public readonly struct RestoreRecord : IDisposable
        {
            public readonly int index;
            public readonly bool isNull;
            public readonly ElementRestoreRecord record;

            public RestoreRecord(int index, bool isNull, ElementRestoreRecord record)
            {
                this.index = index;
                this.isNull = isNull;
                this.record = record;
            }

            public void Dispose()
            {
                record?.Dispose();
            }
        }
    }
}