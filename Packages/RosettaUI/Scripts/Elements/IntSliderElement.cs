namespace RosettaUI
{
    public class IntSliderElement : SliderElement<int>
    {
        private static readonly IGetter<int> MinGetterDefault = ConstGetter.Create(0);
        private static readonly IGetter<int> MaxGetterDefault = ConstGetter.Create(100);

        public IntSliderElement(LabelElement label, IBinder<int> binder, IGetter<int> minGetter, IGetter<int> maxGetter)
            : base(label, binder, minGetter ?? MinGetterDefault, maxGetter ?? MaxGetterDefault)
        {
        }
    }
}