namespace RosettaUI
{
    public class FloatSliderElement : SliderElement<float>
    {
        public static readonly IGetter<float> MinGetterDefault = ConstGetter.Create(0f);
        public static readonly IGetter<float> MaxGetterDefault = ConstGetter.Create(1f);

        public FloatSliderElement(LabelElement label, IBinder<float> binder, SliderOption<float> option)
            : base(label, binder, option.SetMinMaxGetterIfNotExist(MinGetterDefault, MaxGetterDefault))
        {
        }
    }
}