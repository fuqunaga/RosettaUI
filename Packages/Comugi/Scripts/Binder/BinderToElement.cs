using System;
using System.Collections;
using System.Linq;


namespace Comugi
{

    public static class BinderToElement
    {
        public static Element CreateElement(IBinder binder)
        {
            switch (binder)
            {
                case BinderBase<int> bb: return new IntField(bb);
                case BinderBase<float> bb: return new FloatField(bb);
                case BinderBase<string> bb: return new StringField(bb);
                case BinderBase<bool> bb: return new BoolField(bb);
                case IGetter<IList> ig: return CreateListElement(ig);
            }

            var valueType = binder.ValueType;
            if (valueType.IsEnum)
            {
                return CreateEnumElement(binder, valueType);
            }


            Func<Element> createElementFunc = null;

            if (UICustom.TryGetElementCreationMethod(valueType, out var custom))
            {
                createElementFunc = () => custom.func(binder.GetObject());
            }
            else if (typeof(IElementCreator).IsAssignableFrom(valueType))
            {
                createElementFunc = () => ((IElementCreator)binder.GetObject()).CreateElement();
            }
            else if (TypeUtility.HasSerializableField(valueType))
            {

                createElementFunc = () => CreateMemberElement(binder, valueType);
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



        static readonly Element nullElement = BinderToElement.CreateElement(ConstBinder.Create("null")).SetInteractable(false);

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

        static Element CreateEnumElement(IBinder binder, Type valueType)
        {
            var binderType = typeof(EnumToIdxBinder<>).MakeGenericType(valueType);
            var enumToIdxBinder = Activator.CreateInstance(binderType, binder) as BinderBase<int>;

            return new Dropdown(enumToIdxBinder, ConstGetter.Create(Enum.GetNames(valueType)));
        }


        static Element CreateMemberElement(IBinder binder, Type valueType)
        {
            var elements = TypeUtility.GetSerializableFieldNames(valueType)
                .Select(fieldName =>
                {
                    var fieldBinder = PropertyOrFieldBinder.CreateWithBinder(binder, fieldName);
                    var elementGroups = UI.Field(fieldName, fieldBinder);

                    return elementGroups;
                });

            var ret = binder.IsOneliner()
                ? new Row(elements)
                : new Column(elements) as Element;

            return ret;
        }

        public static Element CreateListElement(IGetter<IList> listBinder, Func<IBinder, string, Element> createItemElement = null)
        {
            return NullGuard(
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
        }
    }
}