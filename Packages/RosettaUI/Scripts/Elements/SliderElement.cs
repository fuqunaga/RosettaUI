namespace RosettaUI
{
    public abstract class SliderElement<T> : RangeFieldElement<T, T>
    {
        public SliderElement(LabelElement label, IBinder<T> binder, IGetter<T> minGetter, IGetter<T> maxGetter) 
            : base(label, binder, minGetter, maxGetter)
        {
        }
    }
}