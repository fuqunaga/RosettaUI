namespace RosettaUI
{
    public abstract class MinMaxSliderElement<T> : SliderBaseElement<MinMax<T>, T>
    {
        public MinMaxSliderElement(LabelElement label, IBinder<MinMax<T>> binder, SliderOption<T> option)
            : base(label, binder, option)
        {
        }
    }
}