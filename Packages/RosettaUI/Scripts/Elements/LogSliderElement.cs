namespace RosettaUI
{
    public class LogSliderElement : SliderElement<float>
    {
        public readonly float logBase;

        public LogSliderElement(LabelElement label, IBinder<float> binder, IGetter<MinMax<float>> minMaxGetter, float logBase=10f)
            : base(label, binder, minMaxGetter)
        {
            this.logBase = logBase;
        }
    }
}