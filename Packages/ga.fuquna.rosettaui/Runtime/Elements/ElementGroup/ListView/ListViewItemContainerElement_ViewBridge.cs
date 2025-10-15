using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RosettaUI.UndoSystem;

namespace RosettaUI
{
    public partial class ListViewItemContainerElement
    {
        public class ListViewItemContainerViewBridge : ElementViewBridge
        {
            private ListViewItemContainerElement Element => (ListViewItemContainerElement)element;
            private IBinder Binder => Element._binder;
            
            public ListViewItemContainerViewBridge(ListViewItemContainerElement element) : base(element)
            {
            }

            public IList GetIList() => ListBinder.GetIList(Binder);
            
            public Element GetOrCreateItemElement(int index) => Element.GetOrCreateItemElement(index);

            public void OnItemIndexChanged(int fromIndex, int toIndex) => Element.OnMoveItemIndex(fromIndex, toIndex);

            [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
            public void OnItemsAdded(IEnumerable<int> indices)
            {
                UndoRecordListItemAdd.Register(Element, indices);
                Element.OnItemsAdded(indices);
            } 

            [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
            public void OnItemsRemoved(IEnumerable<int> indices)
            {
                UndoRecordListItemRemove.Record(Element, indices);
                Element.OnItemsRemoved(indices);
            }

            // UIでのリストの参照先の変更を通知
            // 値や要素数の変更は別途OnViewListValueChanged()が呼ばれるのでこちらではNotifyしない
            public void OnViewListChanged(IList list) => Element.SetViewListWithoutNotify(list);

            // UIでのリストの変更を通知
            // 要素数、値
            public void OnViewListValueChanged(IList list) => Element.SetViewList(list);
            
            
            // UIではない外部でのリストの変更を通知
            // 参照or要素数
            public void SubscribeListChanged(Action<IList> action)
            {
                Element.onListChanged += action;
                onUnsubscribe += () => Element.onListChanged -= action;
            }
        }

    }
}