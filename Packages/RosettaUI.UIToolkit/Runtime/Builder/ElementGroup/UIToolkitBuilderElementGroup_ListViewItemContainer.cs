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
            // listView.reorderable = option.reorderable;
            // listView.reorderMode = option.reorderable ? ListViewReorderMode.Animated : ListViewReorderMode.Simple;
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
                
                // Debug.Log($"Bind Idx[{idx}] Value[{e.Query<ReadOnlyValueElement<int>>().First().Value}] Open[{e.Query<FoldElement>().First().IsOpen}]");
                
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
                
                // Debug.Log($"Unbind Idx[{idx}] Value[{e?.Query<ReadOnlyValueElement<int>>().First().Value}] Open[{e?.Query<FoldElement>().First().IsOpen}]");
                
                if (e == null) return;
                
                // UnbindItemでVisualElementに影響を与えてはダメそう
                // 最後尾にスクロールしたときに隙間ができる（本来表示されるVisualElementが内容がない→height==0→非表示になっている
                // e.SetEnable(false);
                Unbind(e);
            }

            void OnItemsRemoved(IEnumerable<int> idxes)
            {
                foreach (var idx in idxes)
                {
                    var e = itemContainerElement.GetContentAt(idx);
                    if (e == null) continue;
                    
                    Unbind(e);
                    e.DetachParent();
                }
            }
            
            void OnItemIndexChanged(int srcIdx, int dstIdx)
            {
                // Debug.Log($"ItemIndexChanged src[{srcIdx}] dst[{dstIdx}]");
                
                CopyFoldOpenClose(srcIdx, dstIdx);
                OnViewListChanged();
            }
            
            // Item が移動したときに Fold の開閉情報を引き継ぐ
            void CopyFoldOpenClose(int srcIdx, int dstIdx)
            {
                if (srcIdx == dstIdx) return;

#if true
                var toElement = itemContainerElement.GetContentAt(srcIdx);
                
                using var pool = ListPool<bool>.Get(out var srcOpenList);
                srcOpenList.AddRange(toElement.Query<FoldElement>().Select(fold => fold.IsOpen));
                
                var sign = (dstIdx < srcIdx) ? -1 : 1;
                var endIdx = dstIdx + sign;
                for (var i = srcIdx + sign; i != endIdx; i += sign)
                {
                    var fromElement = itemContainerElement.GetContentAt(i);
                    Debug.Log($"from[{i}] to[{i-sign}] {fromElement.Query<FoldElement>().First().IsOpen}");
                    
                    foreach(var (from, to) in fromElement.Query<FoldElement>().Zip(toElement.Query<FoldElement>(), (from, to) => (from, to)))
                    {
                        if (from == null || to == null) break;
                        to.SetOpenFlag(from.IsOpen);
                    }

                    toElement = fromElement;
                }

                foreach (var (to, isOpen) in itemContainerElement.GetContentAt(dstIdx).Query<FoldElement>()
                             .Zip(srcOpenList, (to, isOpen) => (to, isOpen)))
                {
                    to.SetOpenFlag(isOpen);
                }
#else
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
#endif
            }

            void OnItemsSourceChanged() => OnViewListChanged();

            void OnItemsSourceSizeChanged()
            {
                lastListItemCount = listView.itemsSource.Count;
                OnViewListChanged();
            }

            void OnElementUpdate(Element _)
            {
                if (float.IsNaN(listView.layout.height)) return;
                
                var list = viewBridge.GetIList();
                
                // ListView 外での参照先変更を ListView に通知
                if (!ReferenceEquals(listView.itemsSource, list))
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
        }
    }
}