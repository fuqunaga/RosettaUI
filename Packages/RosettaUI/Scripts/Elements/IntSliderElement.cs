namespace RosettaUI
{
    public class IntSliderElement : SliderElement<int>
    {
        public IntSliderElement(LabelElement label, BinderBase<int> binder, IGetter<(int, int)> minMaxGetter) : base(label, binder, minMaxGetter ?? ConstGetter.Create((0,100)))
        {
        }
    }
}