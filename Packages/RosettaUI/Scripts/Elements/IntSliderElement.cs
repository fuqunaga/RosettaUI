namespace RosettaUI
{
    public class IntSliderElement : SliderElement<int>
    {
        public IntSliderElement(LabelElement label, IBinder<int> binder, IGetter<MinMax<int>> minMaxGetter) 
            : base(label, binder, minMaxGetter ?? ConstMinMaxGetter.DefaultInt)
        {
        }
    }
}