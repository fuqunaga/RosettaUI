using System;
using System.Linq;
using RosettaUI.Reactive;
using UnityEngine;

namespace RosettaUI
{
    public static class BinderToElement
    {
        #region Field
        
        public static Element CreateFieldElement(LabelElement label, IBinder binder, in FieldOption option)
        {
            var valueType = binder.ValueType;


            if (BinderHistory.IsCircularReference(binder))
            {
                return CreateCircularReferenceElement(label, valueType);
            }

            using var binderHistory = BinderHistory.GetScope(binder);
            var optionCaptured = option;

            return binder switch
            {
                _ when UICustom.GetElementCreationMethod(valueType) is { } creationFunc => InvokeCreationFunc(label,
                    binder, creationFunc),

                IBinder<int> ib => new IntFieldElement(label, ib, option),
                IBinder<uint> ib => new UIntFieldElement(label, ib, option),
                IBinder<float> ib => new FloatFieldElement(label, ib, option),
                IBinder<string> ib => new TextFieldElement(label, ib, option),
                IBinder<bool> ib => new ToggleElement(label, ib),
                IBinder<Color> ib => new ColorFieldElement(label, ib),
                _ when valueType.IsEnum => CreateEnumElement(label, binder),
                _ when TypeUtility.IsNullable(valueType) => CreateNullableFieldElement(label, binder, option),

                _ when binder.GetObject() is IElementCreator elementCreator => WrapNullGuard(() =>
                    elementCreator.CreateElement(label)),
                _ when ListBinder.IsListBinder(binder) => CreateListView(label, binder),

                _ => WrapNullGuard(() => CreateMemberFieldElement(label, binder, optionCaptured))
            };

            Element WrapNullGuard(Func<Element> func) => UI.NullGuardIfNeed(label, binder, func);
        }

        private static Element InvokeCreationFunc(LabelElement label, IBinder binder, UICustom.CreationFunc creationFunc)
        {
            return UI.NullGuardIfNeed(label, binder, () => creationFunc.func(label, binder));
        }

        private static Element CreateEnumElement(LabelElement label, IBinder binder)
        {
            var valueType = binder.ValueType;
            var enumToIdxBinder = EnumToIdxBinder.Create(binder);

            return new DropdownElement(label, enumToIdxBinder, Enum.GetNames(valueType));
        }
        
        private static Element CreateNullableFieldElement(LabelElement label, IBinder binder, FieldOption option)
        {
            var valueBinder = NullableToValueBinder.Create(binder);
            return UI.NullGuard(label, binder, () => CreateFieldElement(label, valueBinder, option));
        }
        
        private static Element CreateListView(LabelElement label, IBinder binder)
        {
            var option = (binder is IPropertyOrFieldBinder pfBinder)
                ? new ListOption(
                    reorderable: TypeUtility.IsReorderable(pfBinder.ParentBinder.ValueType, pfBinder.PropertyOrFieldName)
                )
                : ListOption.Default;


            return UI.List(label, binder, null, option);
        }

        private static Element CreateMemberFieldElement(LabelElement label, IBinder binder, in FieldOption option)
        {
            var valueType = binder.ValueType;
            var optionCaptured = option;

            var elements = TypeUtility.GetUITargetFieldNames(valueType).Select(fieldName =>
            {
                var fieldBinder = PropertyOrFieldBinder.Create(binder, fieldName);
                var fieldLabel = UICustom.ModifyPropertyOrFieldLabel(valueType, fieldName);

                var range = TypeUtility.GetRange(valueType, fieldName);
                if (range != null)
                {
                    var (minGetter, maxGetter) = RangeUtility.CreateGetterMinMax(range, fieldBinder.ValueType);
                    return UI.Slider(fieldLabel, fieldBinder, minGetter, maxGetter);
                }
               
                var field = UI.Field(fieldLabel, fieldBinder, optionCaptured);
                
                
                if (TypeUtility.IsMultiline(valueType, fieldName) && field is TextFieldElement textField)
                {
                    textField.IsMultiLine = true;
                }

                return field;
            });


            Element ret;
            if (TypeUtility.IsSingleLine(valueType))
                ret = new CompositeFieldElement(label, elements);
            else if (label != null)
                ret = UI.Fold(label, elements);
            else
                ret = UI.Column(elements);

            return ret;
        }

        #endregion
        

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
                
                _ when TypeUtility.IsNullable(binder.ValueType) => CreateNullableSliderElement(label, binder, option),
                
                _ => CreateCompositeSliderElement(label, binder, option)
                     ?? CreateFieldElement(label, binder, FieldOption.Default)
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

        private static Element CreateNullableSliderElement(LabelElement label, IBinder binder, SliderOption option)
        {
            var valueBinder = NullableToValueBinder.Create(binder);
            return UI.NullGuard(label, binder, () => CreateSliderElement(label, valueBinder, option));
        }


        private static Element CreateCompositeSliderElement(LabelElement label, IBinder binder, SliderOption option)
        {
            return CreateCompositeSliderElementBase(
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
        
        
        private static Element CreateCompositeSliderElementBase(LabelElement label, IBinder binder, Type valueType,
            Func<string, Element> createFieldElementFunc)
        {
            var fieldNames = TypeUtility.GetUITargetFieldNames(valueType).ToList();
            if (!fieldNames.Any()) return null;

            if (BinderHistory.IsCircularReference(binder))
            {
                return CreateCircularReferenceElement(label, valueType);
            }
            using var binderHistory = BinderHistory.GetScope(binder);

            var elements = fieldNames.Select(createFieldElementFunc);

            return UI.NullGuardIfNeed(label, binder, CreateElementFunc);
            

            // NullGuard前にelements.ToList()などで評価していまうとbinder.Get()のオブジェクトがnullであるケースがある
            // 評価を遅延させる
            Element CreateElementFunc()
            {
                if (TypeUtility.IsSingleLine(binder.ValueType))
                {
                    var titleField = CreateMemberFieldElement(new LabelElement(label), binder, FieldOption.Default);
                    
                    // Foldが閉じてるときは titleField を、開いているときは label を表示
                    // UI.Row(label, titleField) だと titleField のラベルがPrefixLabel判定されないので
                    // 下記の順番である必要がある
                    var bar = UI.Row(titleField, label);

                    // ReSharper disable once PossibleMultipleEnumeration
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
                    // ReSharper disable once PossibleMultipleEnumeration
                    return UI.Fold(label, elements);
                }
            }
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
            return CreateCompositeSliderElementBase(
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

        
        private static Element CreateCircularReferenceElement(LabelElement label, Type type)
        {
            return new CompositeFieldElement(label,
                new[]
                {
                    new HelpBoxElement($"[{type}] Circular reference detected.", HelpBoxType.Error)
                }).SetInteractable(false);
        }
    }
}