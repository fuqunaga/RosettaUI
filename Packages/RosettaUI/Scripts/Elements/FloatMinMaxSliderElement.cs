namespace RosettaUI
{
    public class FloatMinMaxSliderElement : MinMaxSliderElement<float>
    {
        public FloatMinMaxSliderElement(LabelElement label, IBinder<MinMax<float>> binder, IGetter<float> minGetter, IGetter<float> maxGetter)
            : base(label, binder, 
                minGetter ?? FloatSliderElement.MinGetterDefault, 
                maxGetter ?? FloatSliderElement.MaxGetterDefault
                )
        {
        }
    }
}