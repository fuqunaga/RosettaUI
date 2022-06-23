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
        private VisualElement Build_ListViewItemContainer(Element element)
        {
            var itemContainerElement = (ListViewItemContainerElement) element;
            var option = itemContainerElement.option;

            var listView = new ListViewCustom(itemContainerElement.GetIList(),
                makeItem: () => new VisualElement(),
                bindItem: BindItem
            )
            {
                reorderable = option.reorderable,
                reorderMode = option.reorderable ? ListViewReorderMode.Animated : ListViewReorderMode.Simple,
                // showFoldoutHeader = true,
                showAddRemoveFooter = !option.fixedSize,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            };
            

            #region Callbacks

            listView.itemsRemoved += OnItemsRemoved;

            listView.itemIndexChanged += (int srcIdx, int dstIdx) =>
            {
                OnItemIndexChanged(srcIdx, dstIdx);
                NotifyValueChanged();
            };

            // list が Array の場合、参照先が変わる
            // ListView 内で変更される場合も Inspector などで ListView 外で変わる場合もある
            // また ListView 外で List の要素数が変わった場合は、ListView に知らせる必要がある
            var lastListItemCount = listView.itemsSource.Count;          
            
            // ListView 内での参照先変更を通知
            listView.itemsSourceChanged += NotifyValueChanged;

            // ListView 内での要素数変更を通知（ListView 外での変更と区別するための処理）
            listView.itemsSourceSizeChanged += () =>
            {
                lastListItemCount = listView.itemsSource.Count;
                NotifyValueChanged();
            };
            
            itemContainerElement.onUpdate += _ =>
            {
                var list = itemContainerElement.GetIList();
                
                // ListView 外での参照先変更を ListView に通知
                if (listView.itemsSource != list)
                {
                    listView.itemsSource = list;
                }
                
                // ListView 外での要素数の変更を通知
                var listItemCount = listView.itemsSource.Count;
                if ( lastListItemCount != listItemCount)
                {
                    var removeItemCount = lastListItemCount - listItemCount;
                    if (removeItemCount > 0)
                    {
                        OnItemsRemoved(Enumerable.Range(listItemCount, removeItemCount));
                    }

                    lastListItemCount = listItemCount;
                    listView.OnListSizeChangedExternal();
                }
            };
            
            // Listの値が変更されていたらSetIList()（内部的にBinder.SetObject()）する
            // UI.List(writeValue, readValue); の readValue を呼んで通知したい
            itemContainerElement.onViewValueChanged += () => itemContainerElement.SetIList(listView.itemsSource);
            
            #endregion

            
            return listView;

            
            #region Local functions
            
            void BindItem(VisualElement ve, int idx)
            {
                ve.Clear();

                var e = itemContainerElement.GetOrCreateItemElement(idx);
                e.SetEnable(true);

                var itemVe = GetUIObj(e);
                if (itemVe == null)
                {
                    itemVe = Build(e);
                    ApplyIndent(itemVe);
                }
                else
                {
                    e.Update();　// 表示前に最新の値をUIに通知
                }

                ve.Add(itemVe);
            }

            void OnItemsRemoved(IEnumerable<int> idxes)
            {
                foreach (var idx in idxes)
                {
                    var e = itemContainerElement.GetOrCreateItemElement(idx);
                    e.SetEnable(false);
                }
            }
            
            // Item が移動したときに Fold の開閉情報を引き継ぐ
            void OnItemIndexChanged(int srcIdx, int dstIdx)
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
                

                void ExecByPair<T, U>(IEnumerable<T> source, IEnumerable<U> destination, Action<T, U> action)
                {
                    foreach (var (s, d) in source.Zip(destination, (s, d) => (s, d)))
                    {
                        action(s, d);
                    }
                }
            }
            
            // List になにか変更があった場合の通知
            // 参照先変更、サイズ変更、アイテムの値変更
            void NotifyValueChanged()
            {
                itemContainerElement.NotifyViewValueChanged();
            }
            
            #endregion
        }
    }
}