namespace RosettaUI
{
    public  static partial class BinderToElement
    {
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
    }
}