namespace Comugi
{
    public class IntSlider : Slider<int>
    {
        public IntSlider(BinderBase<int> binder, IGetter<(int, int)> minMaxGetter) : base(binder, minMaxGetter ?? ConstGetter.Create((1,100)))
        {
        }
    }
}