namespace RosettaUI
{
    public class FloatMinMaxSliderElement : MinMaxSliderElement<float>
    {
        public FloatMinMaxSliderElement(LabelElement label, BinderBase<MinMax<float>> binder, IMinMaxGetter<float> minMaxGetter)
            : base(label, binder, minMaxGetter ?? ConstMinMaxGetter.DefaultFloat)
        {
        }
    }
}