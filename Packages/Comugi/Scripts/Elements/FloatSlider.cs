namespace Comugi
{
    public class FloatSlider : Slider<float>
    {
        public FloatSlider(BinderBase<float> binder, IGetter<(float, float)> minMaxGetter) : base(binder, minMaxGetter ?? ConstGetter.Create((0f,1f)))
        {
        }
    }
}