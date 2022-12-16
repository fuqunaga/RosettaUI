using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
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
        
        private readonly Dictionary<int, (Element element, IListItemBinder itemBinder)> _itemIndexToElementAndBinder = new();

        protected int ListItemCount => ListBinder.GetCount(_binder);
        
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
            _itemIndexToElementAndBinder.TryGetValue(index, out var pair);
            return pair.element;
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
            RegisterItemElementCache((element, itemBinder), index);

            return element;
        }

        private void RegisterItemElementCache((Element element, IListItemBinder itemBinder) pair, int index)
        {
            Assert.IsFalse(_itemIndexToElementAndBinder.ContainsKey(index));
            
            pair.itemBinder.Index = index;
            _itemIndexToElementAndBinder[index] = pair;
        }

        private void RemoveItemElementCache(int index)
        {
            if (!_itemIndexToElementAndBinder.Remove(index, out var pair)) return;

            var element = pair.element;
            element.DetachView();
            element.DetachParent();
        }

        private void RemoveItemElementCacheAll()
        {
            foreach (var element in _itemIndexToElementAndBinder.Values.Select(pair => pair.element))
            {
                element.DetachView();
                element.DetachParent();
            }

            _itemIndexToElementAndBinder.Clear();
        }


        private Element AddPopupMenu(Element element, IBinder binder, int idx)
        {
            return new PopupMenuElement(
                element,
                () => new[]
                {
                    new MenuItem("Add Element", DuplicateItem),
                    new MenuItem("Remove Element", RemoveItem),
                }
            );

            void DuplicateItem()
            {
                ListBinder.DuplicateItem(binder, idx);
                // OnItemIndexShiftPlus(idx + 1);
                throw new NotImplementedException();
            }

            void RemoveItem()
            {
                ListBinder.RemoveItem(binder, idx);
                // OnItemIndexShiftMinus(idx);
                throw new NotImplementedException();
            }
        }
        
        #region Item Index Changed

        private void OnItemIndexShiftPlus(int startIndex, int endIndex = -1)
        {
            if (endIndex < 0) endIndex = ListBinder.GetCount(_binder) - 1;

            for (var i = endIndex; i > startIndex; --i)
            {
                var prevIndex = i - 1;
                if (!_itemIndexToElementAndBinder.Remove(prevIndex, out var pair)) return;
                RegisterItemElementCache(pair, i);
            }
        }

        private void OnItemIndexShiftMinus(int startIndex, int endIndex = -1)
        {
            if (endIndex < 0) endIndex = ListBinder.GetCount(_binder) - 1;

            for (var i = startIndex; i < endIndex; ++i)
            {
                var prevIndex = i + 1;
                if (!_itemIndexToElementAndBinder.Remove(prevIndex, out var pair)) return;
                RegisterItemElementCache(pair, i);
            }
        }

        private void OnMoveItemIndex(int fromIndex, int toIndex)
        {
            var hasElement = _itemIndexToElementAndBinder.Remove(fromIndex, out var pair);
            if ( toIndex < fromIndex )
                OnItemIndexShiftPlus(toIndex, fromIndex);
            else
                OnItemIndexShiftMinus(fromIndex, toIndex);


            if (hasElement)
            {
                RegisterItemElementCache(pair, toIndex);
            }
        }
 
        #endregion


        private void OnItemsAdded(IEnumerable<int> indices)
        {
            using var pool = ListPool<int>.Get(out var indexList);
            indexList.AddRange(indices.Distinct());

            //　最後尾ならIndexの移動なし
            var itemCountBefore = ListItemCount - indexList.Count;
            if (indexList.All(i => i >= itemCountBefore))
            {
                return;
            }
            
            // 最後尾ではないけどずらす
            if (indexList.Count() == 1)
            {
                var i = indexList.First();
                OnItemIndexShiftPlus(i);
                
                return;
            }
            
            // 最後尾でないかつ複数ならリセット
            RemoveItemElementCacheAll();
        }
        
        private void OnItemsRemoved(IEnumerable<int> indices)
        {
            using var pool = ListPool<int>.Get(out var indexList);
            indexList.AddRange(indices.Distinct());

            //　最後尾ならIndexの移動なし
            var itemCount = ListItemCount;
            if (indexList.All(i => i >= itemCount))
            {
                foreach (var i in indexList)
                {
                    RemoveItemElementCache(i);
                }

                return;
            }
            
            // 最後尾ではないけど１つだけなら消してずらす
            if (indexList.Count() == 1)
            {
                var i = indexList.First();
                RemoveItemElementCache(i);
                OnItemIndexShiftMinus(i);
                
                return;
            }
            
            // 最後尾でないかつ複数ならリセット
            RemoveItemElementCacheAll();
        }

        public override void Update()
        {
            base.Update();

            Debug.Log(
                string.Join(", ",
                    _itemIndexToElementAndBinder.Select(pair => $"{pair.Key}:({pair.Value.element?.FirstLabel()?.Value}, {pair.Value.itemBinder.Index})")
                )
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

            public void RemoveItemElementAfter(int startIndex) => throw new NotImplementedException();

            public void OnItemIndexChanged(int fromIndex, int toIndex) => Element.OnMoveItemIndex(fromIndex, toIndex);

            public void OnItemsAdded(IEnumerable<int> indices) => Element.OnItemsAdded(indices);
            public void OnItemsRemoved(IEnumerable<int> indices) => Element.OnItemsRemoved(indices);
        }
    }

    public static partial class ElementViewBridgeExtensions
    {
        public static ListViewItemContainerElement.ListViewItemContainerViewBridge GetViewBridge(this ListViewItemContainerElement element) => (ListViewItemContainerElement.ListViewItemContainerViewBridge)element.ViewBridge;
    }
}
