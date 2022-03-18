namespace RosettaUI
{
    public class IntSliderElement : SliderElement<int>
    {
        public static readonly IGetter<int> MinGetterDefault = ConstGetter.Create(0);
        public static readonly IGetter<int> MaxGetterDefault = ConstGetter.Create(100);

        public IntSliderElement(LabelElement label, IBinder<int> binder, SliderOption<int> option)
            : base(label, binder, option.SetMinMaxGetterIfNotExist(MinGetterDefault, MaxGetterDefault))
        {
        }
    }
}