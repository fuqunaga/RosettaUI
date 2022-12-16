using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_ListViewItemContainer(Element element, VisualElement visualElement)
        {
            if (element is not ListViewItemContainerElement itemContainerElement ||
                visualElement is not ListViewCustom listView) return false;
            
            var option = itemContainerElement.option;           
            var viewBridge = itemContainerElement.GetViewBridge();
            var itemsSource = viewBridge.GetIList();

            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight; //　これだけ定数
            listView.reorderable = option.reorderable;
            listView.reorderMode = option.reorderable ? ListViewReorderMode.Animated : ListViewReorderMode.Simple;
            listView.showAddRemoveFooter = !option.fixedSize;
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.unbindItem = UnbindItem;


            
            // list が Array の場合、参照先が変わる
            // ListView 内で変更される場合も Inspector などで ListView 外で変わる場合もある
            // また ListView 外で List の要素数が変わった場合は、ListView に知らせる必要がある
            // 外部での要素数の変更を検知するために要素数を保存しといてチェックする
            var lastListItemCount = itemsSource.Count;
            
            SetCallbacks();

            listView.itemsSource = itemsSource;
            
            return true;
            
            #region Local functions

            void SetCallbacks()
            {
                listView.itemsAdded += OnItemsAdded;
                listView.itemsRemoved += OnItemsRemoved;
                listView.itemIndexChanged += OnItemIndexChanged;

                // ListView 内での参照先変更を通知
                listView.itemsSourceChanged += OnItemsSourceChanged;

                // ListView 内での要素数変更を通知（ListView 外での変更と区別するための処理）
#if UNITY_2022_1_OR_NEWER
                listView.viewController.itemsSourceSizeChanged += OnItemsSourceSizeChanged;
#else
                listView.itemsSourceSizeChanged += OnItemsSourceSizeChanged;
#endif

                itemContainerElement.onUpdate += OnElementUpdate;


                viewBridge.onUnsubscribe += () =>
                {
                    listView.itemsRemoved -= OnItemsAdded;
                    listView.itemsRemoved -= OnItemsRemoved;
                    listView.itemIndexChanged -= OnItemIndexChanged;
                    listView.itemsSourceChanged -= OnItemsSourceChanged;
#if UNITY_2022_1_OR_NEWER
                    listView.viewController.itemsSourceSizeChanged -= OnItemsSourceSizeChanged;
#else
                    listView.itemsSourceSizeChanged -= OnItemsSourceSizeChanged;
#endif

                    itemContainerElement.onUpdate -= OnElementUpdate;

                    listView.itemsSource = Array.Empty<int>(); // null だとエラーになるので空配列で

                };
            }
            
            // 各要素ごとに空のVisualElementを作り、
            // その子供にElementの型に応じたVisualElementを生成する
            // Bind対象のElementが変わったら型ごとのVisualElementは非表示にして再利用を待つ
            VisualElement MakeItem()
            {
                var itemVe = new VisualElement();
                ApplyIndent(itemVe); // リストの要素は見栄えを気にしてとりあえず強制インデント

                return itemVe;
            }
            
            void BindItem(VisualElement ve, int idx)
            {
                var e = viewBridge.GetOrCreateItemElement(idx);
                
                Debug.Log($"Bind Idx[{idx}] FirstLabel[{e.FirstLabel()?.Value}]");
                
                e.SetEnable(true);
                e.Update();　// 表示前に最新の値をUIに通知

                var targetVe = ve.Children().FirstOrDefault();
                var success = Bind(e, targetVe);
                if (success) return;
                
                var itemVe = Build(e);
                ve.Add(itemVe);
            }

            void UnbindItem(VisualElement _, int idx)
            {
                var e = itemContainerElement.GetItemElementAt(idx);
                
                Debug.Log($"Unbind Idx[{idx}] FirstLabel[{e?.FirstLabel()?.Value}]");
                
                if (e == null) return;
                
                // UnbindItemでVisualElementに影響を与えてはダメそう
                // 最後尾にスクロールしたときに隙間ができる（本来表示されるVisualElementが内容がない→height==0→非表示になっている
                // e.SetEnable(false);
                Unbind(e);
            }

            // リストの最後への追加しかこないはず
            void OnItemsAdded(IEnumerable<int> idxes)
            {
                viewBridge.OnItemsAdded(idxes);
                OnViewListChanged();
            }
            
            // 複数選択できないので１つか、最後の要素の複数削除しかこないはず
            void OnItemsRemoved(IEnumerable<int> idxes)
            {
                viewBridge.OnItemsRemoved(idxes);
                OnViewListChanged();
            }
            
            void OnItemIndexChanged(int srcIdx, int dstIdx)
            {
                viewBridge.OnItemIndexChanged(srcIdx, dstIdx);
                OnViewListChanged();
            }

            void OnItemsSourceChanged() => OnViewListChanged();

            void OnItemsSourceSizeChanged()
            {
                lastListItemCount = listView.itemsSource.Count;
                OnViewListChanged();
            }

            void OnElementUpdate(Element _)
            {
                var list = viewBridge.GetIList();
                var listItemCount = list.Count;

                // ListView 外での参照先変更を ListView に通知
                if (!ReferenceEquals(listView.itemsSource, list))
                {
                    viewBridge.RemoveItemElementAfter(0);
                    lastListItemCount = listItemCount;
                    listView.itemsSource = list;
                    
                }
                // ListView 外での要素数が変更された
                // 要素のElementを消してlistViewをリフレッシュ
                else if (lastListItemCount != listItemCount)
                {
                    // viewBridge.RemoveItemElementAfter(0);
                    lastListItemCount = listItemCount;
                    listView.RefreshItems(); 
                    Debug.Log($"ListView Count changed externally");
                }
            }
            
            // List になにか変更があった場合の通知
            // 参照先変更、サイズ変更、アイテムの値変更
            void OnViewListChanged()
            {
                // Listの値が変更されていたらSetIList()（内部的にBinder.SetObject()）する
                // itemsSourceは自動的に変更されているが、UI.List(writeValue, readValue); の readValue を呼んで通知したいので手動で呼ぶ
                viewBridge.SetIList(listView.itemsSource);
                itemContainerElement.NotifyViewValueChanged();
            }
            
            #endregion
        }
    }
}