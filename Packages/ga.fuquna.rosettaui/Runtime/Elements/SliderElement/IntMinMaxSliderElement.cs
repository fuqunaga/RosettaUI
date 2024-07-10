namespace RosettaUI
{
    public class IntMinMaxSliderElement : MinMaxSliderElement<int>
    {
        public IntMinMaxSliderElement(LabelElement label, IBinder<MinMax<int>> binder, in SliderElementOption<int> elementOption)
            : base(label, binder, elementOption.SetMinMaxGetterIfNotExist(IntSliderElement.MinGetterDefault,IntSliderElement.MaxGetterDefault))
        {
        }
    }
}