namespace RosettaUI
{
    public class IntSlider : Slider<int>
    {
        public IntSlider(LabelElement label, BinderBase<int> binder, IGetter<(int, int)> minMaxGetter) : base(label, binder, minMaxGetter ?? ConstGetter.Create((1,100)))
        {
        }
    }
}