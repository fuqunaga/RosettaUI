namespace Comugi
{
    public class LogSlider : Slider<float>
    {
        public readonly float logBase;

        public LogSlider(BinderBase<float> binder, IGetter<(float, float)> minMaxGetter, float logBase=10f) : base(binder, minMaxGetter)
        {
            this.logBase = logBase;
        }
    }
}