namespace RosettaUI
{
    public class FloatSliderElement : SliderElement<float>
    {
        public FloatSliderElement(LabelElement label, BinderBase<float> binder, IGetter<MinMax<float>> minMaxGetter) :
            base(label, binder, minMaxGetter ?? ConstMinMaxGetter.DefaultFloat)
        {
        }
    }
}