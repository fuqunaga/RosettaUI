using System;
using System.Linq;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public  static partial class BinderToElement
    {
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
    }
}