namespace RosettaUI
{
    public abstract class MinMaxSliderElement<T> : RangeFieldElement<(T, T), T>
    {
        public MinMaxSliderElement(LabelElement label, BinderBase<(T, T)> binder, IGetter<(T, T)> minMaxGetter) : base(label, binder, minMaxGetter)
        {
        }
    }
}