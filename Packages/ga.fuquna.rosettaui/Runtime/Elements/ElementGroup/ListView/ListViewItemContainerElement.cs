using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace RosettaUI
{
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
    public partial class ListViewItemContainerElement : ElementGroup
    {
        private readonly IBinder _binder;
        public readonly ListViewOption option;
        
        private readonly BinderHistory.Snapshot _binderTypeHistorySnapshot;
        private readonly Dictionary<int, IBinder> _itemIndexToBinder = new();
        private readonly Dictionary<int, Element> _itemIndexToElement = new();
        private readonly Dictionary<int, ElementState> _itemIndexToElementState = new();
        private int _lastListItemCount;
        private IList _lastList;
        
        private event Action<IList> onListChanged;

        
        private ListBinder ListBinder => new(_binder, option.createItemInstanceFunc);
        private IList CurrentList => ListBinder.GetIList(_binder);
        
        public int ListItemCount
        {
            get => ListBinder.GetCount();
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

                ListBinder.SetCount(value);
                NotifyListChangedToView();
            }
        }

        [Obsolete("Use ListViewOption.createItemElementFunc instead")]
        public ListViewItemContainerElement(IBinder listBinder,
            Func<IBinder, int, Element> createItemElement,
            in ListViewOption option) : this(listBinder, new ListViewOption(option){ createItemElementFunc = createItemElement })
        {
        }

        public ListViewItemContainerElement(IBinder listBinder, in ListViewOption option) : base(null)
        {
            _binder = listBinder;
            this.option = option;

            Interactable = !ListBinder.IsReadOnly(listBinder);
            
            _binderTypeHistorySnapshot = BinderHistory.Snapshot.Create();

            _lastList = CurrentList;
            _lastListItemCount = ListItemCount;
        }

        protected override void UpdateInternal()
        {
            if (_lastListItemCount != ListItemCount || !ReferenceEquals(_lastList, CurrentList))
            {
                StoreElementStateAll();
                RemoveItemElementAll();
                NotifyListChangedToView();
            }
            
            base.UpdateInternal();
        }

        private void NotifyListChangedToView()
        {
            _lastList = CurrentList;
            _lastListItemCount = ListItemCount;
            onListChanged?.Invoke(CurrentList);
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

            element = option.createItemElementFunc(itemBinder, index);
            if (!isReadOnly && !option.fixedSize)
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
                    new MenuItem("Add Element", () => GetListEditor().DuplicateItem(index)),
                    new MenuItem("Remove Element", () => GetListEditor().RemoveItem(index))
                }
            );
        }
        
        private void AddItemElement(Element element, int index, bool removeState = true)
        {
            RemoveItemElement(index, removeState);
            _itemIndexToElement[index] = element;
            AddChild(element);
        }

        private void RemoveItemElement(int index, bool removeState = true)
        {
            if (removeState)
            {
                if (_itemIndexToElementState.Remove(index, out var state))
                {
                    state.Dispose();
                }
            }

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
            var state = PopOrCreateElementState(fromIndex);
            SetElementState(toIndex, state);
        }

        private void OnMoveItemIndex(int fromIndex, int toIndex)
        {
            var state = PopOrCreateElementState(fromIndex);
            
            if ( toIndex < fromIndex )
                OnItemIndexShiftPlus(toIndex, fromIndex);
            else
                OnItemIndexShiftMinus(fromIndex, toIndex);
            
            SetElementState(toIndex, state);
        }
        
        
        /// <summary>
        /// 指定したIndexのElementStateを取り出し、なければ新規作成して返す
        /// 取り出したElementStateは辞書から削除される
        /// </summary>
        private ElementState PopOrCreateElementState(int index)
        {
            return _itemIndexToElementState.Remove(index, out var state) 
                ? state 
                : CreateElementState(index);
        }
        
        private void SetElementState(int index, ElementState state)
        {
            if (_itemIndexToElementState.Remove(index, out var oldState))
            {
                oldState.Dispose();
            }

            if (state == null) return;
            _itemIndexToElementState[index] = state;
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
            // 後ろのindexから消してずらす、を繰り返す
            foreach(var i in indices.OrderBy(i => i))
            {
                OnItemIndexShiftMinus(i);
            }
        }
        
        // UI側でListの参照先、要素数、値が変わった(Arrayの要素数変更などすると参照先が変わる）ときの通知
        // UI.List(writeValue, readValue); の readValue を呼んで通知したいので手動で呼ぶ
        private void SetViewListWithoutNotify(IList list)
        {
            _binder.SetObject(list);
            _lastListItemCount = list.Count;
        }

        private void SetViewList(IList list)
        {
            SetViewListWithoutNotify(list);
            NotifyViewValueChanged();
        }
    }
}
