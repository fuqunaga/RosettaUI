namespace RosettaUI
{
    public class FloatMinMaxSliderElement : MinMaxSliderElement<float>
    {
        public FloatMinMaxSliderElement(LabelElement label, IBinder<MinMax<float>> binder, SliderOption<float> option)
            : base(label, binder, option.SetMinMaxGetterIfNotExist(FloatSliderElement.MinGetterDefault, FloatSliderElement.MaxGetterDefault))
        {
        }
    }
}