using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    /// アプリケーション側で変更があった場合、要素の追加削除やリスト自体の参照先が変わったケースなど
    /// どういった変更があったのかわからない
    /// したがってリストの要素数と参照をチェックしておき変化があったらUIを作り直す
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
        private readonly IBinder _binder;
        public readonly ListViewOption option;
        
        private readonly Func<IBinder, int, Element> _createItemElement;
        private readonly BinderHistory.Snapshot _binderTypeHistorySnapshot;
        private readonly Dictionary<int, IBinder> _itemIndexToBinder = new();
        private readonly Dictionary<int, Element> _itemIndexToElement = new();
        private readonly Dictionary<int, ElementState> _itemIndexToElementState = new();
        private int _lastListItemCount;
        private event Action<IList> onListChanged;

        public int ListItemCount
        {
            get => ListBinder.GetCount(_binder);
            set
            {
                var prevCount = ListItemCount;
                if (prevCount == value) return;
                
                // remove
                if (prevCount > value)
                {
                    for (var i = value; i < prevCount; ++i)
                    {
                        RemoveItemElement(i);
                    }
                }

                ListBinder.SetCount(_binder, value);
                NotifyListChangedToView();
            }
        }

        public ListViewItemContainerElement(IBinder listBinder, Func<IBinder, int, Element> createItemElement, ListViewOption option) : base(null)
        {
            _binder = listBinder;
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
                StoreElementStateAll();
                RemoveItemElementAll();
                NotifyListChangedToView();
            }
            
            base.UpdateInternal();
        }

        protected void NotifyListChangedToView()
        {
            _lastListItemCount = ListItemCount;
            onListChanged?.Invoke(ListBinder.GetIList(_binder));
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

            if (!_itemIndexToBinder.TryGetValue(index, out var itemBinder))
            {
                _itemIndexToBinder[index] = itemBinder = ListBinder.CreateItemBinderAt(_binder, index);
            }

            element = _createItemElement(itemBinder, index);
            if (!isReadOnly)
            {
                element = AddPopupMenu(element, index);
            }

            if (_itemIndexToElementState.TryGetValue(index, out var state))
            {
                state.Apply(element);
            }
            
            AddItemElement(element, index);

            return element;
        }
        
        private Element AddPopupMenu(Element element, int index)
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
                ListBinder.DuplicateItem(_binder, index);
                OnItemIndexShiftPlus(index);
                
                NotifyListChangedToView();
            }

            void RemoveItem()
            {
                ListBinder.RemoveItem(_binder, index);
                OnItemIndexShiftMinus(index);

                NotifyListChangedToView();
            }
        }
        
        private void AddItemElement(Element element, int index, bool removeState = true)
        {
            RemoveItemElement(index, removeState);
            _itemIndexToElement[index] = element;
            AddChild(element);
        }

        private void RemoveItemElement(int index, bool removeState = true)
        {
            if ( removeState) _itemIndexToElementState.Remove(index);
            if (!_itemIndexToElement.Remove(index, out var element)) return;

            RemoveChild(element, false);
        }
        
        private void RemoveItemElementAll()
        {
            foreach (var element in _itemIndexToElement.Values)
            {
                RemoveChild(element, false);
            }
            
            _itemIndexToElement.Clear();
        }

        private void StoreElementStateAll()
        {
            foreach (var (index, element) in _itemIndexToElement)
            {
                if (_itemIndexToElementState.ContainsKey(index)) continue;

                _itemIndexToElementState[index] = ElementState.Create(element);
            }
        }

        
        #region Item Index Changed

        private void OnItemIndexShiftPlus(int startIndex, int endIndex = -1)
        {
            if (endIndex < 0) endIndex = ListItemCount - 1;

            for (var i = endIndex; i > startIndex; --i)
            {
                MoveElementState(i - 1, i);
                RemoveItemElement(i, false);
            }
            
            RemoveItemElement(startIndex);
        }

        private void OnItemIndexShiftMinus(int startIndex, int endIndex = -1)
        {
            //すでにListの要素が削除された状態なので、StateとElementはListItemCount+1個の配列に対応している
            if (endIndex < 0) endIndex = ListItemCount;

            for (var i = startIndex; i < endIndex; ++i)
            {
                MoveElementState(i + 1, i);
                RemoveItemElement(i, false);
            }
            
            RemoveItemElement(endIndex);
        }

        private void MoveElementState(int fromIndex, int toIndex)
        {
            if (!_itemIndexToElementState.Remove(fromIndex, out var state))
            {
                state = CreateElementState(fromIndex);
            }

            if (state != null)
            {
                _itemIndexToElementState[toIndex] = state;
            }
        }

        private void OnMoveItemIndex(int fromIndex, int toIndex)
        {
            if (!_itemIndexToElementState.Remove(fromIndex, out var state))
            {
                state = CreateElementState(fromIndex);
            }
            
            if ( toIndex < fromIndex )
                OnItemIndexShiftPlus(toIndex, fromIndex);
            else
                OnItemIndexShiftMinus(fromIndex, toIndex);


            if (state != null)
            {
                _itemIndexToElementState[toIndex] = state;
            }
        }

        private ElementState CreateElementState(int index)
        {
            var element = GetItemElementAt(index);
            return element == null 
                ? null
                : ElementState.Create(element);
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
            // 現状歯抜け選択には非対応
            if (indexList.Count() == 1)
            {
                var i = indexList.First();
                OnItemIndexShiftPlus(i);
                
                return;
            }
            
            // 最後尾でないかつ複数ならリセット
            RemoveItemElementAll();
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
                    RemoveItemElement(i);
                }

                return;
            }
            
            // 最後尾ではないけど１つだけなら消してずらす
            // 現状歯抜け選択には非対応
            if (indexList.Count() == 1)
            {
                var i = indexList.First();
                OnItemIndexShiftMinus(i);
                
                return;
            }
            
            // 最後尾でないかつ複数ならリセット
            RemoveItemElementAll();
        }
        
        // UI側でListの参照先が変わった(Arrayの要素数変更などすると変わる）
        // UI.List(writeValue, readValue); の readValue を呼んで通知したいので手動で呼ぶ
        private void OnViewListChanged(IList list)
        {
            _binder.SetObject(list);
            _lastListItemCount = list.Count;
            NotifyViewValueChanged();
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
                Element.onListChanged += action;
                onUnsubscribe += () => Element.onListChanged -= action;
            }
        }


        // 別のElementに引き継ぐElementの状態
        //　現状FoldのOpen/Close情報のみ
        private class ElementState
        {
            private List<bool> _openList;
            
            public static ElementState Create(Element element)
            {
                return new ElementState()
                {
                    _openList = element.Query<FoldElement>().Select(fold => fold.IsOpen).ToList()
                };
            }

            public void Apply(Element element)
            {
                using var pool = ListPool<FoldElement>.Get(out var foldList);
                foldList.AddRange(element.Query<FoldElement>());
                
                // 数が合わなくてもできるだけ引き継ぐ
                var count = Mathf.Min(foldList.Count, _openList.Count);
                for (var i = 0; i < count; ++i)
                {
                    foldList[i].IsOpen = _openList[i];
                }
            }
        }
    }

    public static partial class ElementViewBridgeExtensions
    {
        public static ListViewItemContainerElement.ListViewItemContainerViewBridge GetViewBridge(this ListViewItemContainerElement element) => (ListViewItemContainerElement.ListViewItemContainerViewBridge)element.ViewBridge;
    }
}
