namespace RosettaUI
{
    public abstract class MinMaxSliderElement<T> : RangeFieldElement<MinMax<T>, T>
    {
        public MinMaxSliderElement(LabelElement label, IBinder<MinMax<T>> binder, IGetter<MinMax<T>> minMaxGetter)
            : base(label, binder, minMaxGetter)
        {
            
        }
    }
}