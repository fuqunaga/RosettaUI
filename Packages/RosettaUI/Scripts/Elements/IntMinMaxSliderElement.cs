namespace RosettaUI
{
    public class IntMinMaxSliderElement : MinMaxSliderElement<int>
    {
        public IntMinMaxSliderElement(LabelElement label, BinderBase<MinMax<int>> binder, IMinMaxGetter<int> minMaxGetter)
            : base(label, binder, minMaxGetter ?? ConstMinMaxGetter.DefaultInt)
        {
        }
    }
}