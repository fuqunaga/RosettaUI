namespace Comugi
{
    public class LogSlider : Slider<float>
    {
        public readonly float logBase;

        public LogSlider(Label label, BinderBase<float> binder, IGetter<(float, float)> minMaxGetter, float logBase=10f) : base(label, binder, minMaxGetter)
        {
            this.logBase = logBase;
        }
    }
}