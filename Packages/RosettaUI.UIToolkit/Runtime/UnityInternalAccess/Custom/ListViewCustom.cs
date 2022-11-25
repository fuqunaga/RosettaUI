#if !UNITY_2022_1_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    /// <summary>
    /// ListView with Additional function
    /// 
    ///  Add item
    ///  - avoid error when IList item is ValueType
    ///  - duplicate a previous item
    /// 
    ///  Support for external List changes
    /// </summary>
    public class ListViewCustom : ListView
    {
        public event Action itemsSourceSizeChanged;

        public ListViewCustom()
        {
            ((ListViewController)GetOrCreateViewController()).itemsSourceSizeChanged += () => itemsSourceSizeChanged?.Invoke();
            
            // disable scroll view
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        }


        #region 画面外 Drag 対策 / サイズの異なるアイテムの移動でScrolViewの高さがおかしくなる対策
        
        /// <summary>
        /// 1. ListViewDragger/ListViewDraggerAnimated はアイテムをドラッグした状態で GameView 外で PointerUpしても認識できず中途半端な状態でDragを継続しようとしてしまう
        /// 2. ListViewDraggerAnimated は高さの異なる Item が２つある状態で移動すると ScrollView 全体の高さがどちらかの2倍？になることがある（すばやく DragAndDrop すると発生しやすい）
        ///
        /// 対策
        /// 1. Drag 中の PointerMove イベントでボタンを押していなければ Drag 中断する対策を行う
        /// 2. OnDrop() で Rebuild() する（要素を一度消してサイズを再計算させる）
        /// </summary>
        
        internal override ListViewDragger CreateDragger() => reorderMode == ListViewReorderMode.Simple ? new ListViewDragger(this) : new ListViewDraggerAnimatedCustom(this);

        private class ListViewDraggerAnimatedCustom : ListViewDraggerAnimated
        {
            // 1.
            public ListViewDraggerAnimatedCustom(BaseVerticalCollectionView listView) : base(listView)
            {
                m_Target.RegisterCallback<PointerMoveEvent>(CheckButtonIsKeptDown);
            }

            private void CheckButtonIsKeptDown(PointerMoveEvent evt)
            {
                if (m_Target.HasPointerCapture(evt.pointerId) && (evt.pressedButtons & 0x01) == 0)
                {
                    using var e = PointerUpEvent.GetPooled(evt);
                    OnPointerUpEvent(e);
                }
            }

            // 2.
            protected override void OnDrop(Vector3 pointerPosition)
            {
                base.OnDrop(pointerPosition);
                
                // Rebuild() で消したアイテムは通常は DynamicHeightVirtualizationController 内で schedule.Execute() で復活するので１フレームアイテムが無い状態で表示されてしまう
                // https://github.com/Unity-Technologies/UnityCsReference/blob/d0fe81a19ce788fd1d94f826cf797aafc37db8ea/ModuleOverrides/com.unity.ui/Core/Collections/Virtualization/DynamicHeightVirtualizationController.cs#L49
                targetListView.Rebuild();

                // Rebuild() で消したアイテムをすぐ復活させるため DynamicHeightVirtualizationController.Fill() を呼びたい
                // 内部で Fill() が呼ばれるように DynamicHeightVirtualizationController.Resize(size,0) を呼ぶ
                var size = targetScrollView.layout.size;
                targetListView.virtualizationController.Resize(size, 0);
            }
        }

        #endregion

        
        #region Avoid error when IList item is ValueType
        
        private protected override void CreateViewController() => SetViewController(new ListViewControllerCustom());

        #endregion
    }
}

#endif