#if !UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;
#endif
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
#if UNITY_2022_1_OR_NEWER
        internal class DynamicHeightVirtualizationControllerCustom : DynamicHeightVirtualizationController<ReusableListViewItem>
        {
            public DynamicHeightVirtualizationControllerCustom(BaseVerticalCollectionView collectionView) : base(collectionView)
            {
            }

            // 最後のアイテムをドラッグすると
            // - ドラッグ中のアイテムは非表示扱い
            // - 総アイテム数より表示アイテムが少ない
            // →表示アイテムを追加
            // となって無限にアイテムを追加してしまうのでドラッグ中のアイテムを非表示扱いにしない
            // https://github.com/Unity-Technologies/UnityCsReference/blob/a6cadb936f3855ab7e5bd8e19d85af403d6802c6/ModuleOverrides/com.unity.ui/Core/Collections/Virtualization/VerticalVirtualizationController.cs#L35-L36
            // https://github.com/Unity-Technologies/UnityCsReference/blob/67987ef0401fa681258a34e708085ec209c95373/ModuleOverrides/com.unity.ui/Core/Collections/Virtualization/DynamicHeightVirtualizationController.cs#L355-L360
            protected override bool VisibleItemPredicate(ReusableListViewItem i)
            {
                return i.rootElement.style.display == DisplayStyle.Flex;
            }
        }
#else
        internal class DynamicHeightVirtualizationControllerCustom : DynamicHeightVirtualizationController<ReusableListViewItem>
        {
            // 初回のFill()時にコンテナのheightがNanである影響でitemsSourceの全アイテム分のVisualElement生成してしまう
            // 大量のVisualElementがあると最初のスクロールなどが重くなる。その後は適切なVisualElement数に戻るので大丈夫
            // 初回Fill()をチェックし一定数以上であれば数を制限してこの現象を回避する
            #region FirstFill

            private const int FirstFillItemCount = 20;
            
            private static FieldInfo _fillCallbackInfo;
            private static FieldInfo _waitingCacheInfo;
            private readonly object _fillOriginalMethod;
            
            public DynamicHeightVirtualizationControllerCustom(BaseVerticalCollectionView collectionView) : base(collectionView)
            {
                _fillCallbackInfo ??= typeof(DynamicHeightVirtualizationController<ReusableListViewItem>).GetField("m_FillCallback",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(_fillCallbackInfo);

                _fillOriginalMethod = _fillCallbackInfo.GetValue(this);
                _fillCallbackInfo.SetValue(this, new Action(FirstFill));
            }

            
            private void FirstFill()
            {
                if (m_ListView.itemsSource.Count < FirstFillItemCount)
                {
                    // call original Fill()
                    ((Action)_fillOriginalMethod).Invoke();
                }
                else
                {
                    for (var i = 0; i < FirstFillItemCount; ++i)
                    {
                        var orMakeItem = this.GetOrMakeItem();
                        this.m_ActiveItems.Add(orMakeItem);
                        this.m_ScrollView.Add(orMakeItem.rootElement);
                        // this.m_WaitingCache.Add(i);
                        AddWaitingCache(i);
                        this.Setup(orMakeItem, i);
                    }
                }

                // Revert m_FillCallback to original Fill()
                _fillCallbackInfo.SetValue(this, _fillOriginalMethod);
            }

            private void AddWaitingCache(int i)
            {
                _waitingCacheInfo ??= typeof(DynamicHeightVirtualizationController<ReusableListViewItem>).GetField("m_WaitingCache",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(_waitingCacheInfo);

                var waitingCache = (HashSet<int>)_waitingCacheInfo.GetValue(this);
                waitingCache.Add(i);
            }
            
            #endregion
        }
#endif
        
}