namespace RosettaUI
{
    public class FloatMinMaxSliderElement : MinMaxSliderElement<float>
    {
        public FloatMinMaxSliderElement(LabelElement label, IBinder<MinMax<float>> binder, IGetter<MinMax<float>> minMaxGetter)
            : base(label, binder, minMaxGetter ?? ConstMinMaxGetter.DefaultFloat)
        {
        }
    }
}