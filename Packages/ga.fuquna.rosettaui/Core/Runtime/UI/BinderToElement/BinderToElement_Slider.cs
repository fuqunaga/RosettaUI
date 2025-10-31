﻿using System;
using System.Linq;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public  static partial class BinderToElement
    {
        public static Element CreateSliderElement(LabelElement label, IBinder binder, in SliderElementOption option)
        {
            var fieldOption = option.sliderOption?.fieldOption ?? FieldOption.Default;
            
            return binder switch
            {
                IBinder<int> ib => new IntSliderElement(label, ib, option.Cast<int>()).AddClipboardMenu(ib, fieldOption),
                IBinder<uint> ib => new IntSliderElement(label,
                    new CastBinder<uint, int>(ib),
                    CreateOptionUintToInt(option)
                ).AddClipboardMenu(ib, fieldOption),
                IBinder<float> ib => new FloatSliderElement(label, ib, option.Cast<float>()).AddClipboardMenu(ib, fieldOption),
                
                _ when TypeUtility.IsNullable(binder.ValueType) => CreateNullableSliderElement(label, binder, option),
                
                _ => CreateCompositeSliderElement(label, binder, option)
                     ?? CreateFieldElement(label, binder, FieldOption.Default)
            };

            static SliderElementOption<int> CreateOptionUintToInt(in SliderElementOption option)
            {
                return new SliderElementOption<int>(
                    CastGetter.Create<uint, int>((IGetter<uint>)option.minGetter),
                    CastGetter.Create<uint, int>((IGetter<uint>)option.maxGetter),
                    option.sliderOption
                );
            }
        }

        private static Element CreateNullableSliderElement(LabelElement label, IBinder binder, SliderElementOption option)
        {
            var valueBinder = NullableToValueBinder.Create(binder);
            return UI.NullGuard(label, binder, () => CreateSliderElement(label, valueBinder, option));
        }


        private static Element CreateCompositeSliderElement(LabelElement label, IBinder binder, SliderElementOption option)
        {
            return CreateCompositeSliderElementBase(
                label,
                binder,
                binder.ValueType,
                option,
                fieldName =>
                {
                    var fieldLabel = UICustom.ModifyPropertyOrFieldLabel(binder.ValueType, fieldName);
                    var fieldBinder = PropertyOrFieldBinder.Create(binder, fieldName);
                    
                    var fieldOption = new SliderElementOption(
                        option,
                        PropertyOrFieldGetter.Create(option.minGetter, fieldName),
                        PropertyOrFieldGetter.Create(option.maxGetter, fieldName)
                    );

                    return UI.Slider(fieldLabel, fieldBinder, fieldOption);
                }
            );
        }
        
        
        private static Element CreateCompositeSliderElementBase(LabelElement label, IBinder binder, Type valueType, in SliderElementOption option,
            Func<string, Element> createFieldElementFunc)
        {
            var fieldNames = TypeUtility.GetUITargetFieldNames(valueType).ToList();
            if (!fieldNames.Any()) return null;

            if (BinderHistory.IsCircularReference(binder))
            {
                return CreateCircularReferenceElement(label, valueType);
            }
            
            using var binderHistory = BinderHistory.GetScope(binder);
            
            var fieldOption = option.sliderOption?.fieldOption ?? FieldOption.Default;
            var isSingleLine = TypeUtility.IsSingleLine(binder.ValueType);

            // NullGuard前にelements.ToList()などで評価していまうとbinder.Get()のオブジェクトがnullであるケースがある
            // elements の評価は遅延させる
            return UI.NullGuardIfNeed(label, binder, isSingleLine ? CreateElementSingleLine : CreateElementMultiLine);
            
            Element CreateElementMultiLine()
            {
                return UI.Fold(label.AddClipboardMenu(binder, fieldOption), fieldNames.Select(createFieldElementFunc));
            }
            
            Element CreateElementSingleLine()
            {
                var titleFieldOption = fieldOption;
                titleFieldOption.suppressClipboardContextMenu = true;
                
                var titleField = CreateMemberFieldElement(new LabelElement(label), binder, titleFieldOption);

                // Foldが閉じてるときは titleField を、開いているときは label を表示
                // UI.Row(label, titleField) だと titleField のラベルがPrefixLabel判定されないので
                // 下記の順番である必要がある
                var bar = UI.Row(titleField, label).AddClipboardMenu(binder, fieldOption);
                    
                var elements = fieldNames.Select(createFieldElementFunc);
                var fold = UI.Fold(bar, elements);

                fold.IsOpenRx.SubscribeAndCallOnce(isOpen =>
                {
                    label.Enable = isOpen;
                    titleField.Enable = !isOpen;
                });

                return fold;
            }
        }
    }
}