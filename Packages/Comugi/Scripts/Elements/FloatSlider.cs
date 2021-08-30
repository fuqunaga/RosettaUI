namespace RosettaUI
{
    public class FloatSlider : Slider<float>
    {
        public FloatSlider(LabelElement label, BinderBase<float> binder, IGetter<(float, float)> minMaxGetter) : base(label, binder, minMaxGetter ?? ConstGetter.Create((0f,1f)))
        {
        }
    }
}