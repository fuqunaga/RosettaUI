using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
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
#if UNITY_2022_1_OR_NEWER
        private static FieldInfo _fieldInfo;

        // Avoid error when IList item is ValueType
        protected override CollectionViewController CreateViewController() => new ListViewControllerCustom();

        private protected override void CreateVirtualizationController()
        {
            _fieldInfo ??= typeof(BaseVerticalCollectionView).GetField("m_VirtualizationController",
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            Assert.IsNotNull(_fieldInfo);
            _fieldInfo.SetValue(this, new DynamicHeightVirtualizationControllerCustom(this));
        }

        private class DynamicHeightVirtualizationControllerCustom : DynamicHeightVirtualizationController<ReusableListViewItem>
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
        public event Action itemsSourceSizeChanged;

        public ListViewCustom()
        {
            ((ListViewController)GetOrCreateViewController()).itemsSourceSizeChanged += () => itemsSourceSizeChanged?.Invoke();
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
#endif
    }
}