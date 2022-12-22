using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    
    /// <summary>
    /// ListViewの要素を表示するエレメント
    /// Foldや要素数フィールドのようなヘッダー要素はほかのElementと組み合わせて実現する
    /// UIToolKitのListViewを想定しており表示領域外のElementはできるだけ生成しない
    /// 
    /// リストに変更があった場合の対応がとてもややこしい
    /// 
    /// アプリケーション側で変更があった場合、追加削除された要素、リスト自体の参照先が変わったケースなど
    /// どういった変更があったのかわからない
    /// したがってリストの要素数と参照をチェックしておき変化があったらUIを作り直すという挙動になっている
    /// この場合要素ElementのFoldElementのOpen/Closeなどの情報は引き継げない
    /// 逆にUI側での変更はわかるのでできるだけ引き継ぐ
    /// 
    /// Reorderableで要素が移動した場合、Binderレベルでは元のリストのインデックスからとれる値が変わるので何もする必要がないが、
    /// 前述のFoldElementのOpen/CloseのようなUIの状態を引き継げない
    /// これに対応するため各IndexのElementとBinderを保持しておき、Binderの参照Indexを新しいIndexに書き換え、
    /// ElementとBinderを新たなIndexのものとして扱うことでUIの状態を引き継げるようにしている
    /// 移動した要素の移動前移動後のIndexだけでなくその間の要素のIndexもすべてずれるのでそれらすべでで上述のIndex替え操作を行う
    /// </summary>
    public class ListViewItemContainerElement : ElementGroup
    {
        public readonly IBinder binder;
        public readonly ListViewOption option;
        
        private readonly Func<IBinder, int, Element> _createItemElement;
        private readonly BinderHistory.Snapshot _binderTypeHistorySnapshot;
        private readonly Dictionary<int, (Element element, IListItemBinder itemBinder)> _itemIndexToElementAndBinder = new();
        private int _lastListItemCount;
        private event Action<IList> _onListChanged;

        public int ListItemCount
        {
            get => ListBinder.GetCount(binder);
            set
            {
                var prevCount = ListItemCount;
                if (prevCount == value) return;
                
                // remove
                if (prevCount > value)
                {
                    for (var i = value; i < prevCount; ++i)
                    {
                        RemoveItemElementCache(i);
                    }
                }

                ListBinder.SetCount(binder, value);
                NotifyListChangedToView();
            }
        }

        public ListViewItemContainerElement(IBinder listBinder, Func<IBinder, int, Element> createItemElement, ListViewOption option) : base(null)
        {
            binder = listBinder;
            _createItemElement = createItemElement;
            this.option = option;

            Interactable = !ListBinder.IsReadOnly(listBinder);
            
            _binderTypeHistorySnapshot = BinderHistory.Snapshot.Create();

            _lastListItemCount = ListItemCount;
        }

        protected override void UpdateInternal()
        {
            var listItemCount = ListItemCount;
            if (_lastListItemCount != listItemCount)
            {
                RemoveItemElementCacheAll();
                NotifyListChangedToView();
            }
            
            base.UpdateInternal();
        }

        protected void NotifyListChangedToView()
        {
            _lastListItemCount = ListItemCount;
            _onListChanged?.Invoke(ListBinder.GetIList(binder));
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
            var isReadOnly = ListBinder.IsReadOnly(binder);
            var itemBinder = ListBinder.CreateItemBinderAt(binder, index);

            element = _createItemElement(itemBinder, index);
            if (!isReadOnly)
            {
                element = AddPopupMenu(element, itemBinder);
            }
            
            AddChild(element);
            RegisterItemElementCache((element, itemBinder), index);

            return element;
        }
        
        private Element AddPopupMenu(Element element, IListItemBinder itemBinder)
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
                var index = itemBinder.Index;
                ListBinder.DuplicateItem(binder, index);
                OnItemIndexShiftPlus(index + 1);
                
                NotifyListChangedToView();
            }

            void RemoveItem()
            {
                var index = itemBinder.Index;
                RemoveItemElementCache(index);
                OnItemIndexShiftMinus(index);
                ListBinder.RemoveItem(binder, index);
                
                NotifyListChangedToView();
            }
        }

        private void RegisterItemElementCache((Element element, IListItemBinder itemBinder) pair, int index)
        {
            Assert.IsFalse(_itemIndexToElementAndBinder.ContainsKey(index), $"{index}");
            
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


        
        #region Item Index Changed

        private void OnItemIndexShiftPlus(int startIndex, int endIndex = -1)
        {
            if (endIndex < 0) endIndex = ListBinder.GetCount(binder) - 1;

            for (var i = endIndex; i > startIndex; --i)
            {
                var prevIndex = i - 1;
                if (!_itemIndexToElementAndBinder.Remove(prevIndex, out var pair)) return;
                RegisterItemElementCache(pair, i);
            }
        }

        private void OnItemIndexShiftMinus(int startIndex, int endIndex = -1)
        {
            if (endIndex < 0) endIndex = ListBinder.GetCount(binder) - 1;

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
            _lastListItemCount = ListItemCount;

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
            _lastListItemCount = ListItemCount;
            
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

        
        // List になにか変更があった場合の通知
        // 参照先変更、サイズ変更、アイテムの値変更
        // Listの値が変更されていたらSetIList()（内部的にBinder.SetObject()）する
        // itemsSourceは自動的に変更されているが、UI.List(writeValue, readValue); の readValue を呼んで通知したいので手動で呼ぶ
        private void OnViewListChanged(IList list)
        {
            binder.SetObject(list);
            _lastListItemCount = list.Count;
            NotifyViewValueChanged();
        }
        
        protected override ElementViewBridge CreateViewBridge() => new ListViewItemContainerViewBridge(this);

        public class ListViewItemContainerViewBridge : ElementViewBridge
        {
            private ListViewItemContainerElement Element => (ListViewItemContainerElement)element;
            private IBinder Binder => Element.binder;
            
            public ListViewItemContainerViewBridge(ListViewItemContainerElement element) : base(element)
            {
            }
            
            public IList GetIList() => ListBinder.GetIList(Binder);
            
            public Element GetOrCreateItemElement(int index) => Element.GetOrCreateItemElement(index);

            public void OnItemIndexChanged(int fromIndex, int toIndex) => Element.OnMoveItemIndex(fromIndex, toIndex);

            public void OnItemsAdded(IEnumerable<int> indices) => Element.OnItemsAdded(indices);
            public void OnItemsRemoved(IEnumerable<int> indices) => Element.OnItemsRemoved(indices);

            // UIでのリストの変更を通知
            // 参照or要素数
            public void OnViewListChanged(IList list) => Element.OnViewListChanged(list);

            // UIではない外部でのリストの変更を通知
            // 参照or要素数
            public void SubscribeListChanged(Action<IList> action)
            {
                Element._onListChanged += action;
                onUnsubscribe += () => Element._onListChanged -= action;
            }
        }
    }

    public static partial class ElementViewBridgeExtensions
    {
        public static ListViewItemContainerElement.ListViewItemContainerViewBridge GetViewBridge(this ListViewItemContainerElement element) => (ListViewItemContainerElement.ListViewItemContainerViewBridge)element.ViewBridge;
    }
}
