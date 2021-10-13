namespace RosettaUI
{
    public abstract class SliderElement<T> : RangeFieldElement<T, T>
    {
        public SliderElement(LabelElement label, IBinder<T> binder, IGetter<MinMax<T>> minMaxGetter) 
            : base(label, binder, minMaxGetter)
        {
        }
    }
}