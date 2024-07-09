namespace RosettaUI
{
    public class FloatMinMaxSliderElement : MinMaxSliderElement<float>
    {
        public FloatMinMaxSliderElement(LabelElement label, IBinder<MinMax<float>> binder, in SliderElementOption<float> elementOption)
            : base(label, binder, elementOption.SetMinMaxGetterIfNotExist(FloatSliderElement.MinGetterDefault, FloatSliderElement.MaxGetterDefault))
        {
        }
    }
}