using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element List<TList>(Expression<Func<TList>> targetExpression, ListViewOption option)
            where TList : IList
            => List(targetExpression, createItemElement: null, option);
        public static Element List<TList>(Expression<Func<TList>> targetExpression, Func<IBinder, int, Element> createItemElement = null, ListViewOption? option = null)
            where TList : IList
            => List(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, createItemElement, option);

        public static Element List<TList>(LabelElement label, Expression<Func<TList>> targetExpression, ListViewOption option)
            where TList : IList
            => List(label, targetExpression, null, option);

        public static Element List<TList>(LabelElement label, Expression<Func<TList>> targetExpression, Func<IBinder, int, Element> createItemElement = null, ListViewOption? option = null)
            where TList : IList
        {
            if (option == null && ExpressionUtility.GetAttribute<TList, NonReorderableAttribute>(targetExpression) != null)
            {
                option = new ListViewOption(reorderable: false, ListViewOption.Default.fixedSize);
            }

            return List(label, ExpressionUtility.CreateBinder(targetExpression), createItemElement, option);
        }

        public static Element List<TList>(Expression<Func<TList>> targetExpression, Action<TList> writeValue, ListViewOption option)
            where TList : IList
            => List(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, option);
        public static Element List<TList>(LabelElement label, Func<TList> readValue, Action<TList> writeValue, ListViewOption option)
            where TList : IList
            => List(label, readValue, writeValue, null, option);
        
        public static Element List<TList>(Expression<Func<TList>> targetExpression, Action<TList> writeValue, Func<IBinder, int, Element> createItemElement = null, ListViewOption? option = null)
            where TList : IList
            => List(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, createItemElement, option);
        public static Element List<TList>(LabelElement label, Func<TList> readValue, Action<TList> writeValue, Func<IBinder, int, Element> createItemElement = null, ListViewOption? option = null) 
            where TList : IList
            => List(label, Binder.Create(readValue, writeValue), createItemElement, option);

        public static Element ListReadOnly<TList>(Expression<Func<TList>> targetExpression, ListViewOption option)
            where TList : IList
            => ListReadOnly(targetExpression, null, option);
        public static Element ListReadOnly<TList>(Expression<Func<TList>> targetExpression, Func<IBinder, int, Element> createItemElement = null, ListViewOption? option = null)
            where TList : IList
        {
            var labelString = ExpressionUtility.CreateLabelString(targetExpression);
            var binder = UIInternalUtility.CreateReadOnlyBinder(targetExpression);
            return List(labelString, binder, createItemElement, option);
        }

        public static Element ListReadOnly<TList>(LabelElement label, Func<TList> readValue, ListViewOption option)
            where TList : IList
            => ListReadOnly(label, readValue, null, option);
        public static Element ListReadOnly<TList>(LabelElement label, Func<TList> readValue, Func<IBinder, int, Element> createItemElement = null, ListViewOption? option = null)
            where TList : IList
        {
            var binder = Binder.Create(readValue, null);
            return List(label, binder, createItemElement, option);
        }
        
        public static Element List(LabelElement label, IBinder listBinder, Func<IBinder, int, Element> createItemElement = null, ListViewOption? optionNullable = null)
        {
            var option = optionNullable ?? ListViewOption.Default;

            var listItemContainer = ListItemContainer(listBinder, createItemElement, option);
            var ret = listItemContainer;
            
            if (option.header)
            {
                var countField = ListCounterField(listBinder, listItemContainer, option);

                ret = Fold(
                    label, countField,
                    new[]
                    {
                        listItemContainer
                    }
                ).Open();
            }

            UIInternalUtility.SetInteractableWithBinder(ret, listBinder);

            return ret;
        }

        public static Element ListCounterField(IBinder listBinder,　Element itemContainerElement, ListViewOption option)
        {
            var interactable = !ListBinder.IsReadOnly(listBinder) && !option.fixedSize;
            
            return Field(null,
                () => ListBinder.GetCount(listBinder),
                count =>
                {
                    // ListViewItemContainerElementが存在していたら新しいcountを通知
                    // NullGuardで存在してない場合もありそのときはlistBinderに直接セットする
                    var containerElement = itemContainerElement.Query<ListViewItemContainerElement>().FirstOrDefault();
                    if (containerElement != null)
                    {
                        containerElement.ListItemCount = count;
                    }
                    else
                    {
                        ListBinder.SetCount(listBinder, count);
                    }
                }).SetMinWidth(50f).SetInteractable(interactable);
        }
        
        private static Element ListItemContainer(IBinder listBinder, Func<IBinder, int, Element> createItemElement, ListViewOption option)
        {
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