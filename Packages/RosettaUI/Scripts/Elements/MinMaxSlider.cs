namespace RosettaUI
{
    public abstract class MinMaxSliderElement<T> : RangeFieldElement<MinMax<T>, T>
    {
        public MinMaxSliderElement(LabelElement label, IBinder<MinMax<T>> binder, IGetter<T> minGetter, IGetter<T> maxGetter)
            : base(label, binder, minGetter, maxGetter)
        {
            
        }
    }
}