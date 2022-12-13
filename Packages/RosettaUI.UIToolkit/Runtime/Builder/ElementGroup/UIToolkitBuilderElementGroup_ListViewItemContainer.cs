using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_ListViewItemContainer(Element element, VisualElement ve)
        {
            if (element is not ListViewItemContainerElement itemContainerElement ||
                ve is not ListViewCustom listView) return false;
            
            var option = itemContainerElement.option;           
            var viewBridge = itemContainerElement.GetViewBridge();

            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight; //　これだけ定数
            listView.reorderable = option.reorderable;
            listView.reorderMode = option.reorderable ? ListViewReorderMode.Animated : ListViewReorderMode.Simple;
            listView.itemsSource = viewBridge.GetIList();
            listView.showAddRemoveFooter = !option.fixedSize;
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.unbindItem = UnbindItem;

            // list が Array の場合、参照先が変わる
            // ListView 内で変更される場合も Inspector などで ListView 外で変わる場合もある
            // また ListView 外で List の要素数が変わった場合は、ListView に知らせる必要がある
            var lastListItemCount = listView.itemsSource.Count;          


            #region Callbacks
            
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
                listView.itemsRemoved -= OnItemsRemoved;
                listView.itemIndexChanged -= OnItemIndexChanged;
                listView.itemsSourceChanged -= OnItemsSourceChanged;
#if UNITY_2022_1_OR_NEWER
                listView.viewController.itemsSourceSizeChanged -= OnItemsSourceSizeChanged;
#else
                listView.itemsSourceSizeChanged -= OnItemsSourceSizeChanged;
#endif
                
                itemContainerElement.onUpdate -= OnElementUpdate;
            };
            
            #endregion

            
            
            #region Local functions

            
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
                e.SetEnable(true);
                e.Update();　// 表示前に最新の値をUIに通知

                var targetVe = ve.Children().FirstOrDefault();
                var success = Bind(e, targetVe);
                if (success) return;
                
                var itemVe = Build(e);
                ApplyIndent(itemVe); 
                ve.Add(itemVe);
            }

            void UnbindItem(VisualElement _, int idx)
            {
                var e = itemContainerElement.GetContentAt(idx);
                if (e == null) return;
                
                e.SetEnable(false); //　itemVeを非表示
                Unbind(e);
            }

            void OnItemsRemoved(IEnumerable<int> idxes)
            {
                foreach (var idx in idxes)
                {
                    var e = itemContainerElement.GetContentAt(idx);
                    Unbind(e);
                    e.DetachParent();
                }
            }
            
            void OnItemIndexChanged(int srcIdx, int dstIdx)
            {
                // CopyFoldOpenClose(srcIdx, dstIdx);
                OnViewListChanged();
            }
            
            // Item が移動したときに Fold の開閉情報を引き継ぐ
            void CopyFoldOpenClose(int srcIdx, int dstIdx)
            {
                if (srcIdx == dstIdx) return;
                
                var src = GetUIObj(itemContainerElement.GetContentAt(srcIdx));              
                var srcFolds = src.Query<Foldout>().Build();
                var srcFoldValues = srcFolds.Select(f => f.value).ToList();

                var veCount = Mathf.Abs(srcIdx - dstIdx) + 1;
                var veArray = ArrayPool<VisualElement>.Shared.Rent(veCount);
                {
                    var isInsert = (dstIdx < srcIdx);
                    var sign = isInsert ? -1 : 1;

                    for (var i = 0; i < veCount; ++i)
                    {
                        var itemIdx = srcIdx + i * sign;
                        veArray[i] = GetUIObj(itemContainerElement.GetContentAt(itemIdx));
                    }

                    for (var i = 0; i < veCount - 1; ++i)
                    {
                        var curr = veArray[i];
                        var prev = veArray[i + 1];
                        
                        var prevFolds = prev.Query<Foldout>().Build();
                        ExecByPair(prevFolds, prevFolds.RebuildOn(curr), (p, c) => c.value = p.value);
                    }

                    ExecByPair(srcFoldValues, srcFolds.RebuildOn(veArray[veCount - 1]), (s, d) => d.value = s);
                }
                ArrayPool<VisualElement>.Shared.Return(veArray);
                

                void ExecByPair<T0, T1>(IEnumerable<T0> source, IEnumerable<T1> destination, Action<T0, T1> action)
                {
                    foreach (var (s, d) in source.Zip(destination, (s, d) => (s, d)))
                    {
                        action(s, d);
                    }
                }
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
                
                // ListView 外での参照先変更を ListView に通知
                if (listView.itemsSource != list)
                {
                    listView.itemsSource = list;
                }
                
                // ListView 外での要素数が変更された
                // 要素のElementを消してlistViewをリフレッシュ
                var listItemCount = listView.itemsSource.Count;
                if (lastListItemCount == listItemCount) return;
                
                var removeItemCount = lastListItemCount - listItemCount;
                if (removeItemCount > 0)
                {
                    OnItemsRemoved(Enumerable.Range(listItemCount, removeItemCount));
                }

                lastListItemCount = listItemCount;
                listView.RefreshItems();
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
            
            
            return true;
        }
    }
}