using System;
using System.Collections;
using System.Linq;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Itemを追加するとき最後のアイテムをコピーするListViewController
    /// </summary>
    public class ListViewControllerCustom : ListViewController
    {
        private readonly Func<IList, Type, int, object> _createItemFunc;
        
        public ListViewControllerCustom(Func<IList, Type, int, object> createItemFunc = null)
        {
            _createItemFunc = createItemFunc;
        }
        
        public override void AddItems(int itemCount)
        {
            if (itemCount <= 0) return;

            var previousCount = GetItemsCount();
            
            var type = itemsSource.GetType();
            var itemType = ListUtility.GetItemType(type);
            for (var i = 0; i < itemCount; ++i)
            {
                if (_createItemFunc != null)
                {
                    var newItem = _createItemFunc(itemsSource, itemType, previousCount + i);
                    itemsSource = ListUtility.AddItemAtLast(itemsSource, itemType, newItem);
                }
                else
                {
                    itemsSource = ListUtility.AddItemAtLast(itemsSource, type, itemType);
                }
            }

            using var _ = ListPool<int>.Get(out var intList);
            intList.AddRange(Enumerable.Range(previousCount, itemCount));
            RaiseItemsAdded(intList);
            
            RaiseOnSizeChanged();
        }
    }
}