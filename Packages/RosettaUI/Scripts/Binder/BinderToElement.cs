using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{
    public static class BinderToElement
    {
        private static readonly BinderBase<string> NullStrBinder = ConstBinder.Create("null");

        public static Element CreateFieldElement(LabelElement label, IBinder binder)
        {
            var valueType = binder.ValueType;

            Func<Element> createElementFunc = binder switch
            {
                _ when UICustom.TryGetElementCreationMethod(valueType, out var custom) => () =>
                    custom.func(binder.GetObject()),

                IBinder<int> bb => () => new IntFieldElement(label, bb),
                IBinder<uint> bb => () => new IntFieldElement(label, new CastBinder<uint, int>(bb), true),
                IBinder<float> bb => () => new FloatFieldElement(label, bb),
                IBinder<string> bb => () => new StringFieldElement(label, bb),
                IBinder<bool> bb => () => new BoolFieldElement(label, bb),
                IBinder<Color> bb => () => new ColorFieldElement(label, bb),
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
                    var fieldBinder = PropertyOrFieldBinder.Create(binder, fieldName);

                    var range = TypeUtility.GetRange(valueType, fieldName);
                    if (range == null)
                    {
                        return UI.Field(fieldName, fieldBinder);
                    }
                    else
                    {
                        var (minGetter, maxGetter) = RangeUtility.CreateGetterMinMax(range, fieldBinder.ValueType);
                        return UI.Slider(fieldName, fieldBinder, minGetter, maxGetter);
                    }
                });


            return binder.IsOneliner()
                ? (Element) new CompositeFieldElement(label, elements)
                : new FoldElement(label, elements);
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

        public static Element CreateSliderElement(LabelElement label, IBinder binder, IGetter minGetter,
            IGetter maxGetter)
        {
            return binder switch
            {
                IBinder<int> bb => new IntSliderElement(label, bb, (IGetter<int>) minGetter, (IGetter<int>) maxGetter),
                IBinder<uint> bb => new IntSliderElement(label,
                    new CastBinder<uint, int>(bb),
                    CastGetter.Create<uint, int>((IGetter<uint>) minGetter),
                    CastGetter.Create<uint, int>((IGetter<uint>) maxGetter)
                ),
                IBinder<float> bb => new FloatSliderElement(label,
                    bb,
                    (IGetter<float>) minGetter,
                    (IGetter<float>) maxGetter
                ),
                _ => CreateCompositeSliderElement(label, binder, minGetter, maxGetter)
                     ?? CreateFieldElement(label, binder)
            };
        }


        private static Element CreateCompositeSliderElement(LabelElement label, IBinder binder, IGetter minGetter,
            IGetter maxGetter)
        {
            return _CreateCompositeSliderElementBase(label, binder,
                binder.ValueType,
                (fieldName) =>
                {
                    var fieldBinder = PropertyOrFieldBinder.Create(binder, fieldName);
                    var fieldMinGetter = PropertyOrFieldGetter.Create(minGetter, fieldName);
                    var fieldMaxGetter = PropertyOrFieldGetter.Create(maxGetter, fieldName);

                    return UI.Slider(fieldName, fieldBinder, fieldMinGetter, fieldMaxGetter);
                });
        }

        #endregion


        #region MinMax Slider

        public static Element CreateMinMaxSliderElement(LabelElement label,
            IBinder binder,
            IGetter minGetter,
            IGetter maxGetter
        )
        {
            return binder switch
            {
                IBinder<MinMax<int>> b => new IntMinMaxSliderElement(label, b,
                    (IGetter<int>) minGetter,
                    (IGetter<int>) maxGetter),
                
                IBinder<MinMax<uint>> b => new IntMinMaxSliderElement(label, 
                    new CastMinMaxBinder<uint,int>(b),
                    (IGetter<int>) minGetter,
                    (IGetter<int>) maxGetter),
                
                IBinder<MinMax<float>> b => new FloatMinMaxSliderElement(label, b,
                    (IGetter<float>) minGetter,
                    (IGetter<float>) maxGetter),
                
                _ => CreateCompositeMinMaxSliderElement(label, binder, minGetter, maxGetter)
            };
        }

        private static Element CreateCompositeMinMaxSliderElement(
            LabelElement label,
            IBinder binder,
            IGetter minGetter,
            IGetter maxGetter
        )
        {
            return _CreateCompositeSliderElementBase(label, binder,
                binder.GetMinMaxValueType(),
                (fieldName) =>
                {
                    var fieldBinder = PropertyOrFieldMinMaxBinder.Create(binder, fieldName);
                    var fieldMinGetter = PropertyOrFieldGetter.Create(minGetter, fieldName);
                    var fieldMaxGetter = PropertyOrFieldGetter.Create(maxGetter, fieldName);

                    return UI.MinMaxSlider(fieldName, fieldBinder, fieldMinGetter, fieldMaxGetter);
                });
        }

        #endregion

        private static Element _CreateCompositeSliderElementBase(LabelElement label, IGetter binder, Type valueType, Func<string, Element> createFieldElementFunc)
        {
            var fieldNames = TypeUtility.GetUITargetFieldNames(valueType).ToList();
            if (!fieldNames.Any()) return null;

            var elements = fieldNames.Select(createFieldElementFunc);

            return NullGuard(label, binder, CreateElementFunc);

            // NullGuard前にelements.ToList()などで評価していまうとbinder.Get()のオブジェクトがnullであるケースがある
            // 評価を遅延させる
            Element CreateElementFunc()
            {
                return new FoldElement(label, elements);
            }
        }
    }
}