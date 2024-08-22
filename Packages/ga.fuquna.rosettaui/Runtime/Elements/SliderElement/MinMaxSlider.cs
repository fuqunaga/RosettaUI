namespace RosettaUI
{
    public abstract class MinMaxSliderElement<T> : SliderBaseElement<MinMax<T>, T>
    {
        public MinMaxSliderElement(LabelElement label, IBinder<MinMax<T>> binder, in SliderElementOption<T> elementOption)
            : base(label, binder, elementOption)
        {
        }
    }
}