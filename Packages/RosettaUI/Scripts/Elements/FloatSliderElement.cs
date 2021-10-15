namespace RosettaUI
{
    public class FloatSliderElement : SliderElement<float>
    {
        public static readonly IGetter<float> MinGetterDefault = ConstGetter.Create(0f);
        public static readonly IGetter<float> MaxGetterDefault = ConstGetter.Create(1f);

        public FloatSliderElement(LabelElement label, IBinder<float> binder, IGetter<float> minGetter, IGetter<float> maxGetter) :
            base(label, binder, minGetter ?? MinGetterDefault, maxGetter ?? MaxGetterDefault)
        {
        }
    }
}