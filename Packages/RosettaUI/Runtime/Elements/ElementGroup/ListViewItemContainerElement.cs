using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace RosettaUI
{
    public class ListViewOption
    {
        private static ListViewOption _default;
        public static ListViewOption Default => _default ?? new (true, false);
        
        public readonly bool reorderable;
        public readonly bool fixedSize;

        public ListViewOption(bool reorderable, bool fixedSize)
        {
            this.reorderable = reorderable;
            this.fixedSize = fixedSize;
        }
    }
    
    
    public class ListViewItemContainerElement : ElementGroup
    {
        private readonly IBinder _binder;
        public readonly ListViewOption option;
        
        private readonly Func<IBinder, int, Element> _createItemElement;
        private readonly BinderHistory.Snapshot _binderTypeHistorySnapshot;
        
        private readonly Dictionary<int, Element> _itemIndexToElement = new();
        
        public ListViewItemContainerElement(IBinder listBinder, Func<IBinder, int, Element> createItemElement, ListViewOption option) : base(null)
        {
            _binder = listBinder;
            _createItemElement = createItemElement;
            this.option = option;

            Interactable = !ListBinder.IsReadOnly(listBinder);
            
            _binderTypeHistorySnapshot = BinderHistory.Snapshot.Create();
        }

        public Element GetItemElementAt(int index)
        {
            _itemIndexToElement.TryGetValue(index, out var element);
            return element;
        }

        private Element GetOrCreateItemElement(int index)
        {
            var element = GetItemElementAt(index);
            if (element != null) return element;
            
            using var applyScope = _binderTypeHistorySnapshot.GetApplyScope();
            var isReadOnly = ListBinder.IsReadOnly(_binder);
            var itemBinder = ListBinder.CreateItemBinderAt(_binder, index);
            
            element = _createItemElement(itemBinder, index);
            if (!isReadOnly)
            {
                element = AddPopupMenu(element, _binder, index);
            }
            
            AddChild(element);
            _itemIndexToElement[index] = element;

            /*
            for (var i = Contents.Count(); i <= index; i++)
            {
                var itemBinder = ListBinder.CreateItemBinderAt(_binder, i);
                element = _createItemElement(itemBinder, i);
                if (!isReadOnly)
                {
                    element = AddPopupMenu(element, _binder, i);
                }
                    
                AddChild(element);
            }
            */

            return element;
        }
        
        private void RemoveItemElementAfter(int startIndex)
        {
            using var pool = ListPool<int>.Get(out var keys);
            keys.AddRange(
                _itemIndexToElement.Keys.Where(key => key >= startIndex)
            );

            foreach (var key in keys)
            {
                if (!_itemIndexToElement.Remove(key, out var element)) continue;
                
                element.DetachView(false);
                element.DetachParent();
            }
        }
        
        private static Element AddPopupMenu(Element element, IBinder binder, int idx)
        {
            return new PopupMenuElement(
                element,
                () => new[]
                {
                    new MenuItem("Add Element", () => ListBinder.DuplicateItem(binder, idx)),
                    new MenuItem("Remove Element", () => ListBinder.RemoveItem(binder, idx)),
                }
            );
        }

        protected override ElementViewBridge CreateViewBridge() => new ListViewItemContainerViewBridge(this);

        public class ListViewItemContainerViewBridge : ElementViewBridge
        {
            private ListViewItemContainerElement Element => (ListViewItemContainerElement)element;
            private IBinder Binder => Element._binder;
            
            public ListViewItemContainerViewBridge(ListViewItemContainerElement element) : base(element)
            {
            }
            
            public IList GetIList() => ListBinder.GetIList(Binder);

            public void SetIList(IList iList) => Binder.SetObject(iList);

            public Element GetOrCreateItemElement(int index) => Element.GetOrCreateItemElement(index);

            public void RemoveItemElementAfter(int startIndex) => Element.RemoveItemElementAfter(startIndex);
        }
    }

    public static partial class ElementViewBridgeExtensions
    {
        public static ListViewItemContainerElement.ListViewItemContainerViewBridge GetViewBridge(this ListViewItemContainerElement element) => (ListViewItemContainerElement.ListViewItemContainerViewBridge)element.ViewBridge;
    }
}
