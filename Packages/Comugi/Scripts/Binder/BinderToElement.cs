using System;
using System.Collections;
using System.Linq;


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
                    var fieldBinder = PropertyOrFieldBinder.CreateWithBinder(binder, fieldName);
                    var elementGroups = UI.Field(fieldName, fieldBinder);

                    return elementGroups;
                });


            Element ret = null;
            if ( binder.IsOneliner())
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
                           return new Column(ListBinder.CreateItemBindersFrom(listBinder).Select(binder => func(binder, "Item" + (i++))));

                       },
                       rebuildIf: (e) =>
                       {
                           var count = listBinder.Get()?.Count ?? 0;
                           return count != (e.element as ElementGroup).Elements.Count;
                       },
                       $"ListEelements({nameof(DynamicElement)})"
                   )
                );

            return label != null
                ? new Row(new[] { label, nullGuard })
                : nullGuard;

        }


#region Slider
        public static Element CreateSliderElement(LabelElement label, IBinder binder, IMinMaxGetter minMaxGetter)
        {
            switch (binder)
            {
                case BinderBase<int> ib: return new IntSlider(label, ib, minMaxGetter as IGetter<(int, int)>);
                case BinderBase<float> ib: return new FloatSliderElement(label, ib, minMaxGetter as IGetter<(float, float)>);

                default:
                    return CreateMemberSliderElement(binder, minMaxGetter);
            }
        }


        static Element CreateMemberSliderElement(IBinder binder, IMinMaxGetter minMaxGetter)
        {
#if false
            var valueType = binder.ValueType;
            var elements = TypeUtility.GetSerializableFieldNames(valueType)
                .Select(memberName =>
                {
                    var memberBinder = PropertyOrFieldBinder.CreateWithBinder(binder, memberName);
                    var memberMinMaxGetter = PropertyOrFieldMinMaxGetter.Create(minMaxGetter, memberName);
                    var elementGroups = UI.Slider(memberName, memberBinder, memberMinMaxGetter);

                    return elementGroups;
                });

            /*
            var ret = oneliner
                ? new Row(elements)
                : new Column(elements) as Element;

                return ret;
            */

            return new Column(elements);
#else
            return null;
#endif
        }


#endregion
    }
}