using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Itemを追加するとき最後のアイテムをコピーするListViewController
    /// </summary>
    public class ListViewControllerCustom : ListViewController
    {
        public override void AddItems(int itemCount)
        {
            if (itemCount <= 0) return;

            var previousCount = GetItemsCount();
            var intList = ListPool<int>.Get();
            try
            {
                var type = itemsSource.GetType();
                var itemType = ListUtility.GetItemType(type);
                for (var i = 0; i < itemCount; ++i)
                {
                    itemsSource = ListUtility.AddItemAtLast(itemsSource, type, itemType);
                    intList.Add(previousCount + i);
                }
                
                RaiseItemsAdded(intList);
            }
            finally
            {
                CollectionPool<List<int>, int>.Release(intList);
            }
            
            RaiseOnSizeChanged();
        }
    }
}