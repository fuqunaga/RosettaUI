using System;
using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element ListItemContainer<TList>(Expression<Func<TList>> targetExpression, ListViewOption option)
            where TList : IList
            => ListItemContainer(targetExpression, null, option);
 
        public static Element ListItemContainer<TList>(Expression<Func<TList>> targetExpression, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
            where TList : IList
        {
            if (option == null && ExpressionUtility.GetAttribute<TList, NonReorderableAttribute>(targetExpression) != null)
            {
                option = new ListViewOption(reorderable: false, ListViewOption.Default.fixedSize);
            }

            return ListItemContainer(ExpressionUtility.CreateBinder(targetExpression), createItemElement, option);
        }

        public static Element ListItemContainer<TList>(Expression<Func<TList>> targetExpression, Action<TList> writeValue, ListViewOption option)
            where TList : IList
            => ListItemContainer(targetExpression.Compile(), writeValue, option);
        public static Element ListItemContainer<TList>(Func<TList> readValue, Action<TList> writeValue, ListViewOption option)
            where TList : IList
            => ListItemContainer(readValue, writeValue, null, option);
        
        public static Element ListItemContainer<TList>(Expression<Func<TList>> targetExpression, Action<TList> writeValue, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
            where TList : IList
            => ListItemContainer(targetExpression.Compile(), writeValue, createItemElement, option);
        public static Element ListItemContainer<TList>(Func<TList> readValue, Action<TList> writeValue, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null) 
            where TList : IList
            => ListItemContainer(Binder.Create(readValue, writeValue), createItemElement, option);

        public static Element ListItemContainerReadOnly<TList>(Expression<Func<TList>> targetExpression, ListViewOption option)
            where TList : IList
            => ListItemContainerReadOnly(targetExpression, null, option);
        public static Element ListItemContainerReadOnly<TList>(Expression<Func<TList>> targetExpression, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
            where TList : IList
        {
            var binder = UIInternalUtility.CreateReadOnlyBinder(targetExpression);
            return ListItemContainer(binder, createItemElement, option);
        }

        public static Element ListItemContainerReadOnly<TList>(Func<TList> readValue, ListViewOption option)
            where TList : IList
            => ListItemContainerReadOnly(readValue, null, option);
        public static Element ListItemContainerReadOnly<TList>( Func<TList> readValue, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
            where TList : IList
        {
            var binder = Binder.Create(readValue, null);
            return ListItemContainer(binder, createItemElement, option);
        }
        
        public static Element ListItemContainer(IBinder listBinder, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
        {
            option ??= ListViewOption.Default;
            
            return NullGuard(null, listBinder,
                () => new ListViewItemContainerElement(
                    listBinder,
                    createItemElement ?? ListItemDefault,
                    option)
            );
        }
        
        public static Element ListItemDefault(IBinder binder, int index) => Field($"Item {index}", binder);
    }
}