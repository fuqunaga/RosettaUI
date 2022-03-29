using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element List<TList>(Expression<Func<TList>> targetExpression, ListViewOption option)
            where TList : IList
            => List(targetExpression, null, option);
        public static Element List<TList>(Expression<Func<TList>> targetExpression, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
            where TList : IList
            => List(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, createItemElement, option);

        public static Element List<TList>(LabelElement label, Expression<Func<TList>> targetExpression, ListViewOption option)
            where TList : IList
            => List(label, targetExpression, null, option);
        public static Element List<TList>(LabelElement label, Expression<Func<TList>> targetExpression, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
            where TList : IList
            => List(label, ExpressionUtility.CreateBinder(targetExpression), createItemElement, option);

        public static Element List<TList>(LabelElement label, Func<TList> readValue, Action<TList> writeValue, ListViewOption option)
            where TList : IList
            => List(label, readValue, writeValue, null, option);
        public static Element List<TList>(LabelElement label, Func<TList> readValue, Action<TList> writeValue, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null) 
            where TList : IList
            => List(label, Binder.Create(readValue, writeValue), createItemElement, option);

        public static Element ListReadOnly<TList>(Expression<Func<TList>> targetExpression, ListViewOption option)
            where TList : IList
            => ListReadOnly(targetExpression, null, option);
        public static Element ListReadOnly<TList>(Expression<Func<TList>> targetExpression, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
            where TList : IList
        {
            var labelString = ExpressionUtility.CreateLabelString(targetExpression);
            var binder = CreateReadOnlyBinder(targetExpression);
            return List(labelString, binder, createItemElement, option);
        }


        public static Element ListReadOnly<TList>(LabelElement label, Func<TList> readValue, ListViewOption option)
            where TList : IList
            => ListReadOnly(label, readValue, null, option);
        public static Element ListReadOnly<TList>(LabelElement label, Func<TList> readValue, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
            where TList : IList
        {
            var binder = Binder.Create(readValue, null);
            return List(label, binder, createItemElement, option);
        }
        
        public static Element List(LabelElement label, IBinder listBinder, Func<IBinder, int, Element> createItemElement = null, ListViewOption option = null)
        {
            option ??= ListViewOption.Default;

            var countField = ListCounterField(listBinder, option);

            return Fold(
                label,countField,
                new[]{
                    ListItemContainer(listBinder, createItemElement, option)
                }
            ).Open();
        }

        public static Element ListCounterField(IBinder listBinder, ListViewOption option = null)
        {
            option ??= ListViewOption.Default;
            var isReadOnly = ListBinder.IsReadOnly(listBinder);
            
            return Field(null,
                () => ListBinder.GetCount(listBinder),
                count => ListBinder.SetCount(listBinder, count)
            ).SetMinWidth(50f).SetInteractable(!isReadOnly && !option.fixedSize);
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


#if false

        public static Element List_(LabelElement label, IBinder listBinder, Func<IBinder, int, Element> createItemElement = null)
        {
            var isReadOnly = ListBinder.IsReadOnly(listBinder);

            var countFieldWidth = 80f;
            var field = Field(null,
                () => ListBinder.GetCount(listBinder),
                isReadOnly ? (Action<int>) null : (count) => ListBinder.SetCount(listBinder, count)
            ).SetWidth(countFieldWidth);
     
            var buttons = isReadOnly
                ? null
                : Row(
                    Space(),
                    Button("＋", () => ListBinder.AddItemAtLast(listBinder)),
                    Button("－", () => ListBinder.RemoveItemAtLast(listBinder))
                );

            return Fold(
                barLeft: label,
                barRight: field,
                elements: new[]
                {
                    Box(Indent(
                        List(listBinder, createItemElement),
                        buttons
                        )
                    )
                }
            );
        }
        
        public static Element List(IBinder listBinder, Func<IBinder, int, Element> createItemElement = null)
        {
            return DynamicElementOnStatusChanged(
                readStatus: () => ListBinder.GetCount(listBinder),
                build: _ =>
                {
                    createItemElement ??= ((binder, idx) => Field("Element " + idx, binder));

                    var itemBinderToElement = createItemElement;

                    var isReadOnly = ListBinder.IsReadOnly(listBinder);
                    if (!isReadOnly)
                    {
                        itemBinderToElement = (binder,idx) => ListViewItemContainerElement.AddPopupMenu(createItemElement(binder, idx), binder, idx);
                    }

                    return Column(
                        ListBinder.CreateItemBinders(listBinder).Select(itemBinderToElement)
                    ).SetInteractable(!isReadOnly);
                });
        }

#endif
    }
}