namespace RosettaUI
{
    public class IntMinMaxSliderElement : MinMaxSliderElement<int>
    {
        public IntMinMaxSliderElement(LabelElement label, IBinder<MinMax<int>> binder, IGetter<MinMax<int>> minMaxGetter)
            : base(label, binder, minMaxGetter ?? ConstMinMaxGetter.DefaultInt)
        {
        }
    }
}