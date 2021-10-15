namespace RosettaUI
{
    public class FloatSliderElement : SliderElement<float>
    {
        public FloatSliderElement(LabelElement label, IBinder<float> binder, IGetter<MinMax<float>> minMaxGetter) :
            base(label, binder, minMaxGetter ?? ConstMinMaxGetter.DefaultFloat)
        {
        }
    }
}