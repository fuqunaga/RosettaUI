namespace RosettaUI
{
    public abstract class SliderElement<T> : RangeFieldElement<T, T>
    {
        public SliderElement(LabelElement label, BinderBase<T> binder, IMinMaxGetter<T> minMaxGetter) 
            : base(label, binder, minMaxGetter)
        {
        }
    }
}