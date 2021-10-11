namespace RosettaUI
{
    public abstract class MinMaxSliderElement<T> : RangeFieldElement<MinMax<T>, T>
    {
        public MinMaxSliderElement(LabelElement label, BinderBase<MinMax<T>> binder, IMinMaxGetter<T> minMaxGetter)
            : base(label, binder, minMaxGetter)
        {
            
        }
    }
}