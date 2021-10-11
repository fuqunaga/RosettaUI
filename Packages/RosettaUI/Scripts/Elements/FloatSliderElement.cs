namespace RosettaUI
{
    public class FloatSliderElement : SliderElement<float>
    {
        public FloatSliderElement(LabelElement label, BinderBase<float> binder, IMinMaxGetter<float> minMaxGetter) :
            base(label, binder, minMaxGetter ?? ConstMinMaxGetter.DefaultFloat)
        {
        }
    }
}