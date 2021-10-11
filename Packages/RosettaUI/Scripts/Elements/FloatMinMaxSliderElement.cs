namespace RosettaUI
{
    public class FloatMinMaxSliderElement : MinMaxSliderElement<float>
    {
        public FloatMinMaxSliderElement(LabelElement label, BinderBase<(float,float)> binder, IGetter<(float, float)> minMaxGetter)
            : base(label, binder, minMaxGetter ?? ConstGetter.Create((0f,1f)))
        {
        }
    }
}