using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{
    public static class BinderToElement
    {
        private static readonly BinderBase<string> NullStrBinder = ConstBinder.Create("null");

        public static Element CreateElement(LabelElement label, IBinder binder)
        {
            var valueType = binder.ValueType;

            Func<Element> createElementFunc = binder switch
            {
                _ when UICustom.TryGetElementCreationMethod(valueType, out var custom) => () =>
                    custom.func(binder.GetObject()),

                BinderBase<int> bb => () => new IntFieldElement(label, bb),
                BinderBase<uint> bb => () => new IntFieldElement(label, new CastBinder<uint, int>(bb), true),
                BinderBase<float> bb => () => new FloatFieldElement(label, bb),
                BinderBase<string> bb => () => new StringFieldElement(label, bb),
                BinderBase<bool> bb => () => new BoolFieldElement(label, bb),
                BinderBase<Color> bb => () => new ColorFieldElement(label, bb),
                _ when binder.ValueType.IsEnum => () => CreateEnumElement(label, binder),

                IGetter<IElementCreator> ig => () => ig.Get().CreateElement(),
                IGetter<IList> ig => () => CreateListElement(label, ig),

                _ => () => CreateMemberFieldElement(label, binder)
            };


            return valueType.IsValueType || valueType == typeof(string)
                ? createElementFunc()
                : NullGuard(label, binder, createElementFunc);
        }

        private static Element NullGuard(LabelElement label, IGetter getter, Func<Element> createElement)
        {
            Element ret = null;

            if (getter.IsNullable && getter.ValueType != typeof(string))
                ret = DynamicElement.Create(
                    () => getter.IsNull,
                    isNull => isNull
                        ? new StringFieldElement(label, NullStrBinder).SetInteractable(false)
                        : createElement(),
                    $"NullGuard({nameof(DynamicElement)})"
                );
            else
                ret = createElement();

            return ret;
        }

        private static Element CreateEnumElement(LabelElement label, IBinder binder)
        {
            var valueType = binder.ValueType;
            var binderType = typeof(EnumToIdxBinder<>).MakeGenericType(valueType);
            var enumToIdxBinder = Activator.CreateInstance(binderType, binder) as BinderBase<int>;

            return new DropdownElement(label, enumToIdxBinder, Enum.GetNames(valueType));
        }


        private static Element CreateMemberFieldElement(LabelElement label, IBinder binder)
        {
            var valueType = binder.ValueType;

            var elements = TypeUtility.GetUITargetFieldNames(valueType)
                .Select(fieldName =>
                {
                    var range = TypeUtility.GetRange(valueType, fieldName);

                    var fieldBinder = PropertyOrFieldBinder.CreateWithBinder(binder, fieldName);

                    var element = range == null
                        ? UI.Field(fieldName, fieldBinder)
                        : UI.Slider(fieldName, fieldBinder, ConstMinMaxGetter.Create(range.min, range.max));

                    return element;
                });

            Element ret = null;
            if (binder.IsOneliner())
                ret = new CompositeFieldElement(label, elements);
            else
                ret = new FoldElement(label, elements);

            return ret;
        }

        public static Element CreateListElement(LabelElement label, IGetter<IList> listBinder,
            Func<IBinder, string, Element> createItemElement = null)
        {
            Element element = new DynamicElement(
                () =>
                {
                    var i = 0;
                    var func = createItemElement ?? ((binder, label) => UI.Field(label, binder));
                    var itemElements = ListBinder.CreateItemBindersFrom(listBinder)
                        .Select(binder => func(binder, "Item" + i++));

                    var listType = listBinder.ValueType;
                    var itemType = TypeUtility.GetListItemType(listBinder.ValueType);


                    var buttonWidth = 30;

                    var addButton =
                        UI.Button("+", () => IListUtility.AddItemAtLast(listBinder.Get(), listType, itemType))
                            .SetMinWidth(buttonWidth);
                    var removeButton = UI.Button("-", () => IListUtility.RemoveItemAtLast(listBinder.Get(), itemType))
                        .SetMinWidth(buttonWidth);

                    return UI.Box(itemElements.Concat(new[]
                        {UI.Row(addButton, removeButton).SetJustify(Layout.Justify.End)}));
                },
                e =>
                {
                    var count = listBinder.Get()?.Count ?? 0;
                    return count !=
                           ((ElementGroup) e.element).Children.Count - 1; // -1 for UI.Row(addButton, removeButton)
                },
                $"ListElements({nameof(DynamicElement)})"
            );

            return label != null
                ? new FoldElement(label, new[] {element})
                : element;
        }


        #region Slider

        public static Element CreateSliderElement(LabelElement label, IBinder binder, IMinMaxGetter minMaxGetter)
        {
            return binder switch
            {
                BinderBase<int> bb => new IntSliderElement(label, bb, (IGetter<(int, int)>) minMaxGetter),
                BinderBase<float> bb => new FloatSliderElement(label, bb, (IGetter<(float, float)>) minMaxGetter),
                _ => CreateCompositeSliderElement(label, binder, minMaxGetter)
            };
        }


        private static Element CreateCompositeSliderElement(LabelElement label, IBinder binder,
            IMinMaxGetter minMaxGetter)
        {
            var fieldNames = TypeUtility.GetUITargetFieldNames(binder.ValueType).ToList();
            if (!fieldNames.Any()) return null;

            var elements = fieldNames.Select(fieldName =>
            {
                var fieldBinder = PropertyOrFieldBinder.CreateWithBinder(binder, fieldName);
                var fieldMinMaxGetter = minMaxGetter != null
                    ? PropertyOrFieldMinMaxGetter.Create(minMaxGetter, fieldName)
                    : null;
                return UI.Slider(fieldName, fieldBinder, fieldMinMaxGetter);
            });

            return binder.IsNullable
                ? NullGuard(label, binder, CreateElementFunc)
                : CreateElementFunc();

            Element CreateElementFunc()
            {
                return new FoldElement(label, elements);
            }
        }

        #endregion


        #region MinMax Slider

        public static Element CreateMinMaxSliderElement(LabelElement label, IBinder binder, IMinMaxGetter minMaxGetter)
        {
            return binder switch
            {
                BinderBase<(int, int)> bb => new IntMinMaxSliderElement(label, bb, (IGetter<(int, int)>) minMaxGetter),
                BinderBase<(float, float)> bb => new FloatMinMaxSliderElement(label, bb, (IGetter<(float, float)>) minMaxGetter),
                //_ => CreateCompositeSliderElement(label, binder, minMaxGetter)
                _ => throw new NotImplementedException()
            };
        }

        #endregion
    }
}