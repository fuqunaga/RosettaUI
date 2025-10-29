using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace RosettaUI
{
    public static partial class UI
    {
        #region targetExpression

        public static Element List<TList> (
            Expression<Func<TList>> targetExpression,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, option);
        }

        [Obsolete("Use ListViewOption to set createItemElementFunc")]
        public static Element List<TList>(
            Expression<Func<TList>> targetExpression,
            Func<IBinder, int, Element> createItemElement,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(targetExpression, MergeOptionWithCreateElementFunc(option, createItemElement));
        }

        public static Element List<TList>(
            LabelElement label,
            Expression<Func<TList>> targetExpression,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(
                label,
                ExpressionUtility.CreateBinder(targetExpression),
                option ?? CalcDefaultOptionOf(targetExpression)
            );
        }

        [Obsolete("Use ListViewOption to set createItemElementFunc")]
        public static Element List<TList>(
            LabelElement label,
            Expression<Func<TList>> targetExpression,
            Func<IBinder, int, Element> createItemElement,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(
                label,
                targetExpression,
                MergeOptionWithCreateElementFunc(option, createItemElement)
            );
        }

        #endregion


        #region readValue/writeValue
        
        public static Element List<TList>(
            Expression<Func<TList>> targetExpression,
            Action<TList> writeValue,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(
                ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression.Compile(), 
                writeValue,
                option ?? CalcDefaultOptionOf(targetExpression));
        }

        [Obsolete("Use ListViewOption to set createItemElementFunc")]
        public static Element List<TList>(
            Expression<Func<TList>> targetExpression,
            Action<TList> writeValue,
            Func<IBinder, int, Element> createItemElement,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(
                targetExpression,
                writeValue,
                MergeOptionWithCreateElementFunc(option, createItemElement)
            );
        }

        public static Element List<TList>(
            LabelElement label,
            Func<TList> readValue,
            Action<TList> writeValue,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(label, Binder.Create(readValue, writeValue), option);
        }

        [Obsolete("Use ListViewOption to set createItemElementFunc")]
        public static Element List<TList>(
            LabelElement label,
            Func<TList> readValue,
            Action<TList> writeValue,
            Func<IBinder, int, Element> createItemElement,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(label, readValue, writeValue, MergeOptionWithCreateElementFunc(option, createItemElement));
        }

        
        #endregion


        #region ReadOnly

        public static Element ListReadOnly<TList>(
            Expression<Func<TList>> targetExpression,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(
                ExpressionUtility.CreateLabelString(targetExpression),
                UIInternalUtility.CreateReadOnlyBinder(targetExpression),
                option ?? CalcDefaultOptionOf(targetExpression));
        }
        
        [Obsolete("Use ListViewOption to set createItemElementFunc")]
        public static Element ListReadOnly<TList>(
            Expression<Func<TList>> targetExpression,
            Func<IBinder, int, Element> createItemElement,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return ListReadOnly(
                targetExpression,
                MergeOptionWithCreateElementFunc(option ?? CalcDefaultOptionOf(targetExpression), createItemElement)
            );
        }
        
        public static Element ListReadOnly<TList>(
            LabelElement label,
            Func<TList> readValue,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            var binder = Binder.Create(readValue, null);
            return List(label, binder, option);
        }
        
        [Obsolete("Use ListViewOption to set createItemElementFunc")]
        public static Element ListReadOnly<TList>(
            LabelElement label, 
            Func<TList> readValue, 
            Func<IBinder, int, Element> createItemElement,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return ListReadOnly(label, readValue, MergeOptionWithCreateElementFunc(option, createItemElement));
        }

        
        #endregion


        #region Core

        private static readonly Dictionary<Type, Func<object, string>> ListItemLabelGetterCache = new();

        public static Element List(LabelElement label, IBinder listBinder, Func<IBinder, int, Element> createItemElement = null, in ListViewOption? optionNullable = null)
        {
            var option = optionNullable ?? ListViewOption.Default;
            option.createItemElementFunc = createItemElement;

            return List(label, listBinder, option);
        }
        
        public static Element List(LabelElement label, IBinder listBinder, in ListViewOption? optionNullable = null)
        {
            var option = optionNullable ?? ListViewOption.Default;
            option.createItemElementFunc ??= ListItemDefault;
            
            var listItemContainer = ListItemContainer(listBinder, option);
            var ret = listItemContainer;
            
            if (option.header)
            {
                var countField = ListCounterField(listBinder, listItemContainer, option);

                ret = Fold(
                    Row(label, Space(), countField).AddClipboardMenu(listBinder, FieldOption.Default),
                    new[]
                    {
                        listItemContainer
                    }
                ).Open();
            }

            UIInternalUtility.SetInteractableWithBinder(ret, listBinder);

            return ret;
        }

        public static Element ListCounterField(IBinder binder,　Element itemContainerElement, in ListViewOption option)
        {
            var listBinder = new ListBinder(binder, option.createItemInstanceFunc);
            var interactable = !listBinder.IsReadOnly() && !option.fixedSize;

            return Field(null,
                () => listBinder.GetCount(),
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
                        listBinder.SetCount(count);
                    }
                },
                new FieldOption { delayInput = true }
            ).SetMinWidth(32f).SetInteractable(interactable);
        }
        
        private static Element ListItemContainer(IBinder listBinder, in ListViewOption option)
        {
            var optionCaptured = option;
            optionCaptured.createItemElementFunc ??= ListItemDefault;
            
            return NullGuard(null, listBinder,
                () => new ListViewItemContainerElement(listBinder, optionCaptured)
            ).SetFlexShrink(1f);
        }

        public static Element ListItemDefault(IBinder binder, int index)
        {
#if !ENABLE_IL2CPP
            var valueType = binder.ValueType;
            if (valueType is { IsClass: true } or { IsValueType: true, IsPrimitive: false, IsEnum: false } && valueType != typeof(string))
            {
                // The first field of the class/struct type is string, use that value as the label
                if (!ListItemLabelGetterCache.TryGetValue(valueType, out var getter))
                {
                    // Search for the first field of string
                    var firstField = TypeUtility.GetUITargetFieldNames(valueType)
                        .Select(n => TypeUtility.GetMemberInfo(valueType, n))
                        .OfType<FieldInfo>()
                        .FirstOrDefault();

                    if (firstField != null && firstField.FieldType == typeof(string))
                    {
                        // Expression Tree: object obj => ((valueType) obj).firstField
                        var inputParam = Expression.Parameter(typeof(object), "obj");   // object obj
                        var getFieldExp = Expression.Field(Expression.Convert(inputParam, valueType), firstField);  // ((valueType) obj).firstField
                        getter = Expression.Lambda<Func<object, string>>(getFieldExp, inputParam).Compile();    // obj => getFieldExp
                    }
                    else
                    {
                        getter = _ => null;
                    }
                    ListItemLabelGetterCache[valueType] = getter;
                }

                return Field(Label(() =>
                {
                    var obj = binder.GetObject();
                    var label = obj == null ? string.Empty : getter(obj);
                    return string.IsNullOrEmpty(label) ? $"Item {index}" : label;
                }), binder);
            }
#endif
            return Field($"Item {index}", binder);
        }

        private static ListViewOption? CalcDefaultOptionOf(LambdaExpression expression)
            => ExpressionUtility.GetAttribute<NonReorderableAttribute>(expression) != null
                    ? new ListViewOption(reorderable: false)
                    : null;

        
        private static ListViewOption MergeOptionWithCreateElementFunc(in ListViewOption? optionNullable, Func<IBinder, int, Element> createItemElement)
        {
            var newOption = optionNullable ?? ListViewOption.Default;
            newOption.createItemElementFunc = createItemElement;
            return newOption;
        }

        #endregion
    }
}