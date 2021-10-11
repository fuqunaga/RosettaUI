namespace RosettaUI
{
    public class IntSliderElement : SliderElement<int>
    {
        public IntSliderElement(LabelElement label, BinderBase<int> binder, IMinMaxGetter<int> minMaxGetter) 
            : base(label, binder, minMaxGetter ?? ConstMinMaxGetter.DefaultInt)
        {
        }
    }
}