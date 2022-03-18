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
        private VisualElement Build_ListView(Element element)
        {
            var listViewElement = (ListViewElement) element;
            var option = listViewElement.option;

            var listView = new ListViewCustom(listViewElement.GetIList(),
                makeItem: () => new VisualElement(),
                bindItem: BindItem
            )
            {
                reorderable = option.reorderable,
                reorderMode = option.reorderable ? ListViewReorderMode.Animated : ListViewReorderMode.Simple,
                showFoldoutHeader = true,
                showAddRemoveFooter = !option.fixedSize,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                unbindItem = UnbindItem
            };

            if (option.fixedSize)
            {
                listView.Q<TextField>().SetEnabled(false);
            }

            ApplyMinusIndentIfPossible(listView, listViewElement);
            listView.ScheduleToUseResolvedLayoutBeforeRendering(() =>
            {
                ApplyIndent(listView.Q<Foldout>().contentContainer);
                listView.Rebuild(); // 起動時にスクロールバーが表示されるのを回避。いまいち原因を追えてない対処療法
            });
            
            listViewElement.Label.SubscribeValueOnUpdateCallOnce(str => listView.headerTitle = str);
            UIToolkitUtility.SetAcceptClicksIfDisabled(listView.Q<Toggle>());

            listView.itemIndexChanged += OnItemIndexChanged;

            
            // list が Array の場合、参照先が変わる
            // ListView 内で変更される場合も Inspector などで ListView 外で変わる場合もある
            // また ListView 外で List の要素数が変わった場合は、ListView に知らせる必要がある
            var lastListItemCount = listView.itemsSource.Count;          
            
            // ListView 内での参照先変更を通知
            listView.itemsSourceChanged += () => listViewElement.SetIList(listView.itemsSource);

            // ListView 内での要素数変更を通知（ListView 外での変更と区別するための処理）
            listView.itemsSourceSizeChanged += () => lastListItemCount = listView.itemsSource.Count;
            
            listViewElement.onUpdate += _=>
            {
                var list = listViewElement.GetIList();
                
                // ListView 外での参照先変更を ListView に通知
                if (listView.itemsSource != list)
                {
                    listView.itemsSource = list;
                }
                
                // ListView 外での要素数の変更を通知
                var listItemCount = listView.itemsSource.Count;
                if ( lastListItemCount != listItemCount)
                {
                    lastListItemCount = listItemCount;
                    listView.OnListSizeChangedExternal();
                }
            };

            return listView;

            
            void BindItem(VisualElement ve, int idx)
            {
                ve.Clear();

                var e = listViewElement.GetOrCreateItemElement(idx);
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
            
            void UnbindItem(VisualElement _, int idx)
            {
                var e = listViewElement.GetOrCreateItemElement(idx);
                e.SetEnable(false);
            }

            // Item が移動したときに Fold の開閉情報を引き継ぐ
            void OnItemIndexChanged(int srcIdx, int dstIdx)
            {
                if (srcIdx == dstIdx) return;
                
                var src = GetUIObj(listViewElement.GetContentAt(srcIdx));              
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
                        veArray[i] = GetUIObj(listViewElement.GetContentAt(itemIdx));
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
        }
    }
}