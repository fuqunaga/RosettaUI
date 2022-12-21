using System;
using System.Collections.Generic;
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
#if !UNITY_2022_1_OR_NEWER
        public event Action itemsSourceSizeChanged;

        public ListViewCustom()
        {
            ((ListViewController)GetOrCreateViewController()).itemsSourceSizeChanged += () => itemsSourceSizeChanged?.Invoke();
        }
#endif
        
        
        
        #region Avoid error when IList item is ValueType
        
#if UNITY_2022_1_OR_NEWER
        
        protected override CollectionViewController CreateViewController() => new ListViewControllerCustom();
#else
        private protected override void CreateViewController() => SetViewController(new ListViewControllerCustom());
#endif

        #endregion
        

        #region DynamicHeightVirtualizationController
        
        private static FieldInfo _fieldInfo;
        private protected override void CreateVirtualizationController()
        {
            if (virtualizationMethod != CollectionVirtualizationMethod.DynamicHeight)
            {
                base.CreateVirtualizationController();
                return;
            }

            _fieldInfo ??= typeof(BaseVerticalCollectionView).GetField("m_VirtualizationController", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(_fieldInfo);
            _fieldInfo.SetValue(this, new DynamicHeightVirtualizationControllerCustom(this));
            // _fieldInfo.SetValue(this, new DynamicHeightVirtualizationControllerForDebug<ReusableListViewItem>(this));
        }

        #endregion
    }
}