namespace RosettaUI
{
    public class LogSliderElement : SliderElement<float>
    {
        public readonly float logBase;

        public LogSliderElement(LabelElement label, BinderBase<float> binder, IGetter<(float, float)> minMaxGetter, float logBase=10f) : base(label, binder, minMaxGetter)
        {
            this.logBase = logBase;
        }
    }
}