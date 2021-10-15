namespace RosettaUI
{
    public class IntMinMaxSliderElement : MinMaxSliderElement<int>
    {
        public IntMinMaxSliderElement(LabelElement label, IBinder<MinMax<int>> binder, IGetter<int> minGetter, IGetter<int> maxGetter)
            : base(label, binder,
                minGetter ?? IntSliderElement.MinGetterDefault,
                maxGetter ?? IntSliderElement.MaxGetterDefault
                )
        {
        }
    }
}