namespace RosettaUI
{
    public class IntSliderElement : SliderElement<int>
    {
        public static readonly IGetter<int> MinGetterDefault = ConstGetter.Create(0);
        public static readonly IGetter<int> MaxGetterDefault = ConstGetter.Create(100);

        public IntSliderElement(LabelElement label, IBinder<int> binder, in SliderElementOption<int> elementOption)
            : base(label, binder, elementOption.SetMinMaxGetterIfNotExist(MinGetterDefault, MaxGetterDefault))
        {
        }
    }
}