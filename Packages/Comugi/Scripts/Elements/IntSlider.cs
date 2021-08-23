namespace Comugi
{
    public class IntSlider : Slider<int>
    {
        public IntSlider(Label label, BinderBase<int> binder, IGetter<(int, int)> minMaxGetter) : base(label, binder, minMaxGetter ?? ConstGetter.Create((1,100)))
        {
        }
    }
}