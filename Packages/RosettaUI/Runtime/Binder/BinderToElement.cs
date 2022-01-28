using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Reactive;
using UnityEngine;

namespace RosettaUI
{
    public static class BinderToElement
    {
        private static readonly BinderBase<string> NullStrBinder = ConstBinder.Create("null");

        private static Element CreateCircularReferenceElement(LabelElement label, Type type) =>
            new CompositeFieldElement(label,
                new[]
                {
                    new HelpBoxElement($"[{type}] Circular reference detected.", HelpBoxType.Error)
                }).SetInteractable(false);
        public static Element CreateFieldElement(LabelElement label, IBinder binder)
        {
            var valueType = binder.ValueType;
            using var typeHistory = BinderTypeHistory.Get(valueType);
            
            if ( typeHistory == null)
            {
                return CreateCircularReferenceElement(label, valueType);
            }

            Func<Element> createElementFunc = binder switch
            {
                _ when UICustom.GetElementCreationMethod(valueType) is { } creationFunc => () =>
                    creationFunc.func(binder.GetObject()),

                IBinder<int> ib => () => new IntFieldElement(label, ib),
                IBinder<uint> ib => () => new UIntFieldElement(label, ib),
                IBinder<float> ib => () => new FloatFieldElement(label, ib),
                IBinder<string> ib => () => new TextFieldElement(label, ib),
                IBinder<bool> ib => () => new BoolFieldElement(label, ib),
                IBinder<Color> ib => () => new ColorFieldElement(label, ib),
                _ when binder.ValueType.IsEnum => () => CreateEnumElement(label, binder),

                _ when binder.GetObject() is IElementCreator elementCreator => elementCreator.CreateElement,
                _ when ListBinder.IsListBinder(binder) => () => UI.List(label, binder),

                _ => () => CreateMemberFieldElement(label, binder)
            };


            return valueType.IsValueType || valueType == typeof(string)
                ? createElementFunc()
                : NullGuard(label, binder, createElementFunc);
        }

        private static Element NullGuard(LabelElement label, IGetter getter, Func<Element> createElement)
        {
            Element ret;

            if (getter.IsNullable && getter.ValueType != typeof(string))
                ret = DynamicElement.Create(
                    () => getter.IsNull,
                    isNull => isNull
                        ? new TextFieldElement(label, NullStrBinder).SetInteractable(false)
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

            var elements = TypeUtility.GetUITargetFieldNames(valueType).Select(fieldName =>
            {
                var fieldBinder = PropertyOrFieldBinder.Create(binder, fieldName);
                var fieldLabel = UICustom.ModifyPropertyOrFieldLabel(valueType, fieldName);

                var range = TypeUtility.GetRange(valueType, fieldName);
                if (range == null)
                {
                    return UI.Field(fieldLabel, fieldBinder);
                }
                else
                {
                    var (minGetter, maxGetter) = RangeUtility.CreateGetterMinMax(range, fieldBinder.ValueType);
                    return UI.Slider(fieldLabel, fieldBinder, minGetter, maxGetter);
                }
            });


            Element ret = null;
            if (TypeUtility.IsSingleLine(binder.ValueType))
                ret = new CompositeFieldElement(label, elements);
            else if (label != null)
                ret = UI.Fold(label, elements);
            else
                ret = UI.Column(elements);

            return ret;
        }

        private static Element _CreateCompositeSliderElementBase(LabelElement label, IBinder binder, Type valueType,
            Func<string, Element> createFieldElementFunc)
        {
            var fieldNames = TypeUtility.GetUITargetFieldNames(valueType).ToList();
            if (!fieldNames.Any()) return null;

            using var typeHistory = BinderTypeHistory.Get(valueType);
            if (typeHistory == null)
            {
                return CreateCircularReferenceElement(label, valueType);
            }

            var elements = fieldNames.Select(createFieldElementFunc);

            return NullGuard(label, binder, CreateElementFunc);

            // NullGuard前にelements.ToList()などで評価していまうとbinder.Get()のオブジェクトがnullであるケースがある
            // 評価を遅延させる
            Element CreateElementFunc()
            {
                if (TypeUtility.IsSingleLine(binder.ValueType))
                {
                    var titleField = CreateMemberFieldElement(new LabelElement(label), binder);
                    var bar = UI.Row(label, titleField);

                    var fold = UI.Fold(bar, elements);
                    
                    fold.IsOpenRx.SubscribeAndCallOnce(isOpen =>
                    {
                        label.Enable = isOpen;
                        titleField.Enable = !isOpen;
                    });

                    return fold;
                }
                else
                {
                    return UI.Fold(label, elements);
                }
            }
        }


        #region Slider

        public static Element CreateSliderElement(LabelElement label, IBinder binder, SliderOption option)
        {
            return binder switch
            {
                IBinder<int> bb => new IntSliderElement(label, bb, option.Cast<int>()),
                IBinder<uint> bb => new IntSliderElement(label,
                    new CastBinder<uint, int>(bb),
                    CreateOptionUintToInt(option)
                ),
                IBinder<float> bb => new FloatSliderElement(label, bb, option.Cast<float>()
                ),
                _ => CreateCompositeSliderElement(label, binder, option)
                     ?? CreateFieldElement(label, binder)
            };

            static SliderOption<int> CreateOptionUintToInt(SliderOption option)
            {
                return new SliderOption<int>()
                {
                    minGetter = CastGetter.Create<uint, int>((IGetter<uint>) option.minGetter),
                    maxGetter = CastGetter.Create<uint, int>((IGetter<uint>) option.maxGetter),
                    showInputField = option.showInputField
                };
            }
        }


        private static Element CreateCompositeSliderElement(LabelElement label, IBinder binder, SliderOption option)
        {
            return _CreateCompositeSliderElementBase(
                label,
                binder,
                binder.ValueType,
                fieldName =>
                {
                    var fieldLabel = UICustom.ModifyPropertyOrFieldLabel(binder.ValueType, fieldName);
                    var fieldBinder = PropertyOrFieldBinder.Create(binder, fieldName);
                    
                    var fieldOption = new SliderOption(option)
                    {
                        minGetter = PropertyOrFieldGetter.Create(option.minGetter, fieldName),
                        maxGetter = PropertyOrFieldGetter.Create(option.maxGetter, fieldName),
                    };


                    return UI.Slider(fieldLabel, fieldBinder, fieldOption);
                }
            );
        }

        #endregion


        #region MinMax Slider

        public static Element CreateMinMaxSliderElement(LabelElement label, IBinder binder, SliderOption option)
        {
            return binder switch
            {
                IBinder<MinMax<int>> b => new IntMinMaxSliderElement(label, b, option.Cast<int>()),

                // IBinder<MinMax<uint>> b => new IntMinMaxSliderElement(label,
                //     new CastMinMaxBinder<uint, int>(b),
                //     (IGetter<int>) minGetter,
                //     (IGetter<int>) maxGetter),
                IBinder<MinMax<uint>> b => null,

                IBinder<MinMax<float>> b => new FloatMinMaxSliderElement(label, b, option.Cast<float>()),

                _ => CreateCompositeMinMaxSliderElement(label, binder, option)
            };
        }

        private static Element CreateCompositeMinMaxSliderElement(LabelElement label, IBinder binder, SliderOption option)
        {
            return _CreateCompositeSliderElementBase(
                label, 
                binder,
                binder.GetMinMaxValueType(),
                fieldName =>
                {
                    var fieldBinder = PropertyOrFieldMinMaxBinder.Create(binder, fieldName);
                    var fieldOption = new SliderOption(option)
                    {
                        minGetter = PropertyOrFieldGetter.Create(option.minGetter, fieldName),
                        maxGetter = PropertyOrFieldGetter.Create(option.maxGetter, fieldName),
                    };

                    return UI.MinMaxSlider(fieldName, fieldBinder, fieldOption);
                });
        }

        #endregion
        

        class BinderTypeHistory : IDisposable
        {
            private static readonly HashSet<Type> History = new();
            public static BinderTypeHistory Get(Type type) => History.Add(type) ? new BinderTypeHistory(type) : null;

            
            private readonly Type _type;

            private BinderTypeHistory(Type type) => _type = type;

            public void Dispose()
            {
                History.Remove(_type);
            }
        }
    }
}