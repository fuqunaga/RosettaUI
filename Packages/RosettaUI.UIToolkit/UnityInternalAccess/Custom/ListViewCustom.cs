using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace UnityInternalAccess.Custom
{
    /// <summary>
    /// ListView that Avoid error when IList item is ValueType
    /// Support for external List changes
    /// </summary>
    public class ListViewCustom : ListView
    {
        public ListViewCustom() : base()
        {
        }

        public ListViewCustom(
            IList itemsSource,
            float itemHeight = -1f,
            Func<VisualElement> makeItem = null,
            Action<VisualElement, int> bindItem = null)
            : base(itemsSource, itemHeight, makeItem, bindItem)
        {
        }

        public void OnListSizeChangedExternal() => RefreshItems();


        #region Avoid error when IList item is ValueType
        
        private protected override void CreateViewController() => SetViewController(new ListViewControllerCustom());

        class ListViewControllerCustom : ListViewController
        {
            private static Array AddToArray(Array source, int itemCount)
            {
                Array instance = Array.CreateInstance(source.GetType().GetElementType() ?? throw new InvalidOperationException("Cannot resize source, because its size is fixed."), source.Length + itemCount);
                Array.Copy(source, instance, source.Length);
                return instance;
            }
            
            private void EnsureItemSourceCanBeResized()
            {
                if (this.itemsSource.IsFixedSize && !this.itemsSource.GetType().IsArray)
                    throw new InvalidOperationException("Cannot add or remove items from source, because its size is fixed.");
            }

            private static readonly Dictionary<Type, object> DefaultValuTable = new();

            public override void AddItems(int itemCount)
            {
                this.EnsureItemSourceCanBeResized();
                int count = this.itemsSource.Count;
                List<int> intList = CollectionPool<List<int>, int>.Get();
                try
                {
                    if (this.itemsSource.IsFixedSize)
                    {
                        this.itemsSource = (IList) ListViewControllerCustom.AddToArray((Array) this.itemsSource, itemCount);
                        for (int index = 0; index < itemCount; ++index)
                            intList.Add(count + index);
                    }
                    else
                    {
                        // Avoid error when IList item is ValueType
#if true
                        var itemSourceType = itemsSource.GetType();
                        if (!DefaultValuTable.TryGetValue(itemSourceType, out var defaultValue))
                        {
                            if (itemSourceType.IsGenericType)
                            {
                                var itemType = itemSourceType.GetGenericArguments()[0];
                                if (itemType.IsValueType)
                                {
                                    defaultValue = Activator.CreateInstance(itemType);
                                }
                            }

                            DefaultValuTable[itemSourceType] = defaultValue;
                        }

                        for (int index = 0; index < itemCount; ++index)
                        {
                            intList.Add(count + index);
                            this.itemsSource.Add(defaultValue);
                        }
#else
                        for (int index = 0; index < itemCount; ++index)
                        {
                            intList.Add(count + index);
                            this.itemsSource.Add((object) null);
                        }
#endif
                    }
                    this.RaiseItemsAdded((IEnumerable<int>) intList);
                }
                finally
                {
                    CollectionPool<List<int>, int>.Release(intList);
                }
                this.RaiseOnSizeChanged();
                if (!this.itemsSource.IsFixedSize)
                    return;
                this.view.Rebuild();
            }
        }
        
        #endregion
    }
}