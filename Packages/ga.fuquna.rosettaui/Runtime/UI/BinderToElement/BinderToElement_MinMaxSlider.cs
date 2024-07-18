﻿namespace RosettaUI
{
    public  static partial class BinderToElement
    {
        public static Element CreateMinMaxSliderElement(LabelElement label, IBinder binder, in SliderElementOption elementOption)
        {
            return binder switch
            {
                IBinder<MinMax<int>> b => new IntMinMaxSliderElement(label, b, elementOption.Cast<int>()),

                // IBinder<MinMax<uint>> b => new IntMinMaxSliderElement(label,
                //     new CastMinMaxBinder<uint, int>(b),
                //     (IGetter<int>) minGetter,
                //     (IGetter<int>) maxGetter),
                IBinder<MinMax<uint>> b => null,

                IBinder<MinMax<float>> b => new FloatMinMaxSliderElement(label, b, elementOption.Cast<float>()),

                _ => CreateCompositeMinMaxSliderElement(label, binder, elementOption)
            };
        }

        private static Element CreateCompositeMinMaxSliderElement(LabelElement label, IBinder binder, SliderElementOption elementOption)
        {
            return CreateCompositeSliderElementBase(
                label, 
                binder,
                binder.GetMinMaxValueType(),
                fieldName =>
                {
                    var fieldBinder = PropertyOrFieldMinMaxBinder.Create(binder, fieldName);
                    var fieldOption = new SliderElementOption(
                        elementOption,
                        PropertyOrFieldGetter.Create(elementOption.minGetter, fieldName),
                        PropertyOrFieldGetter.Create(elementOption.maxGetter, fieldName)
                    );

                    return UI.MinMaxSlider(fieldName, fieldBinder, fieldOption);
                });
        }
    }
}