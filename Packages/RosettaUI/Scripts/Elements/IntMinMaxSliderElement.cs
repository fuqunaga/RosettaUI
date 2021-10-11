namespace RosettaUI
{
    public class IntMinMaxSliderElement : MinMaxSliderElement<int>
    {
        public IntMinMaxSliderElement(LabelElement label, BinderBase<(int,int)> binder, IGetter<(int, int)> minMaxGetter)
            : base(label, binder, minMaxGetter ?? ConstGetter.Create((0,100)))
        {
        }
    }
}