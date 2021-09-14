using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{

    public static class BinderToElement
    {
        public static Element CreateElement(LabelElement label, IBinder binder)
        {
            var element = binder switch
            {
                BinderBase<int> bb => new IntFieldElement(label, bb),
                BinderBase<uint> bb => new IntFieldElement(label, new CastBinder<uint, int>(bb), true),
                BinderBase<float> bb => new FloatFieldElement(label, bb),
                BinderBase<string> bb => new StringFieldElement(label, bb),
                BinderBase<bool> bb => new BoolFieldElement(label, bb),
                IGetter<IList> ig => CreateListElement(label, ig),
                _ => null
            };

            if (element != null)
            {
                return element;
            }

            var valueType = binder.ValueType;
            if (valueType.IsEnum)
            {
                return CreateEnumElement(label, binder, valueType);
            }


            Func<Element> createElementFunc = null;

            if (UICustom.TryGetElementCreationMethod(valueType, out var custom))
            {
                createElementFunc = () => custom.func(binder.GetObject());
            }
            else if (typeof(IElementCreator).IsAssignableFrom(valueType))
            {
                createElementFunc = () => new FoldElement(label, new[] { ((IElementCreator)binder.GetObject()).CreateElement() });
            }
            else if (TypeUtility.HasSerializableField(valueType))
            {
                createElementFunc = () => CreateCompositeFieldElement(label, binder, valueType);
            }

            if (createElementFunc != null)
            {
                if (valueType.IsValueType)
                {
                    return createElementFunc();
                }
                else
                {
                    return NullGuard(binder, createElementFunc);
                }
            }

            return null;
        }



        static readonly Element nullElement = new StringFieldElement(null, ConstBinder.Create("null")).SetInteractable(false);

        static Element NullGuard(IGetter getter, Func<Element> createElement)
        {
            Element ret = null;

            if (getter.IsNullable && getter.ValueType != typeof(string))
            {
                ret = DynamicElement.Create(
                    readStatus: () => getter.IsNull,
                    buildWithStatus: (isNull) =>
                    {
                        return isNull ? nullElement : createElement();
                    },
                    $"NullGuard({nameof(DynamicElement)})"
                );
            }
            else
            {
                ret = createElement();
            }

            return ret;
        }

        static Element CreateEnumElement(LabelElement label, IBinder binder, Type valueType)
        {
            var binderType = typeof(EnumToIdxBinder<>).MakeGenericType(valueType);
            var enumToIdxBinder = Activator.CreateInstance(binderType, binder) as BinderBase<int>;

            return new DropdownElement(label, enumToIdxBinder, Enum.GetNames(valueType));
        }


        static Element CreateCompositeFieldElement(LabelElement label, IBinder binder, Type valueType)
        {
            var elements = TypeUtility.GetSerializableFieldNames(valueType)
                .Select(fieldName =>
                {
                    var range = TypeUtility.GetRange(valueType, fieldName);

                    var fieldBinder = PropertyOrFieldBinder.CreateWithBinder(binder, fieldName);

                    Element element = (range == null)
                    ? UI.Field(fieldName, fieldBinder)
                    : UI.Slider(fieldName, fieldBinder, ConstMinMaxGetter.Create(range.min, range.max));

                    return element;
                });


            Element ret = null;
            if (binder.IsOneliner())
            {
                ret = new CompositeFieldElement(label, new Row(elements));
            }
            else
            {
                ret = new FoldElement(label, elements);

            }

            return ret;
        }

        public static Element CreateListElement(LabelElement label, IGetter<IList> listBinder, Func<IBinder, string, Element> createItemElement = null)
        {
            var nullGuard = NullGuard(
                listBinder,
                () => new DynamicElement(
                       build: () =>
                       {
                           var i = 0;
                           var func = createItemElement ?? ((binder, label) => UI.Field(label, binder));
                           var itemElements = ListBinder.CreateItemBindersFrom(listBinder).Select(binder => func(binder, "Item" + (i++)));

                           var listType = listBinder.ValueType;
                           var itemType = TypeUtility.GetListItemType(listBinder.ValueType);

                           var addButton = UI.Button("+", () => IListUtility.AddItemAtLast(listBinder.Get(), listType, itemType));
                           var removeButton = UI.Button("-", () => IListUtility.RemoveItemAtLast(listBinder.Get(), itemType));

                           return UI.Box(itemElements.Concat(new[] { UI.Row(addButton, removeButton)}));

                       },
                       rebuildIf: (e) =>
                       {
                           var count = listBinder.Get()?.Count ?? 0;
                           return count != (e.element as ElementGroup).Elements.Count - 1; // -1 for UI.Row(addButton, removeButton)
                       },
                       $"ListEelements({nameof(DynamicElement)})"
                   )
                );

            return label != null
                ? new FoldElement(label, new[] { nullGuard })
                : nullGuard;


        }


        #region Slider
        public static Element CreateSliderElement(LabelElement label, IBinder binder, IMinMaxGetter minMaxGetter)
        {
            Element element = binder switch
            {
                BinderBase<int> bb => new IntSliderElement(label, bb, (IGetter<(int, int)>)minMaxGetter),
                BinderBase<float> bb => new FloatSliderElement(label, bb, (IGetter<(float, float)>)minMaxGetter),
                _ => null
            };

            if (element != null)
            {
                return element;
            }

            var valueType = binder.ValueType;
            if (TypeUtility.HasSerializableField(valueType))
            {
                return CreateCompositeSliderElement(label, binder, minMaxGetter, binder.ValueType);
            }


            // Sliderに出来ないものはCreateElement()へ
            return CreateElement(label, binder);
        }


        static Element CreateCompositeSliderElement(LabelElement label, IBinder binder, IMinMaxGetter minMaxGetter, Type valueType)
        {
            var elements = TypeUtility.GetSerializableFieldNames(valueType)
                .Select(fieldName =>
                {
                    var fieldBinder = PropertyOrFieldBinder.CreateWithBinder(binder, fieldName);
                    var fieldMinMaxGetter = minMaxGetter != null ? PropertyOrFieldMinMaxGetter.Create(minMaxGetter, fieldName) : null;
                    return UI.Slider(fieldName, fieldBinder, fieldMinMaxGetter);
                });


            return new FoldElement(label, elements);
        }


        #endregion
    }
}