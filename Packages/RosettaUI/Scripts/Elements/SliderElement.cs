namespace RosettaUI
{
    public abstract class SliderElement<T> : RangeFieldElement<T, T>
    {
        public SliderElement(LabelElement label, BinderBase<T> binder, IGetter<(T,T)> minMaxGetter) : base(label, binder, minMaxGetter)
        {
        }
    }
}