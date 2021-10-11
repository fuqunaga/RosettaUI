namespace RosettaUI
{
    public class FloatSliderElement : SliderElement<float>
    {
        public FloatSliderElement(LabelElement label, BinderBase<float> binder, IGetter<(float, float)> minMaxGetter) :
            base(label, binder, minMaxGetter ?? ConstGetter.Create((0f, 1f)))
        {
        }
    }
}