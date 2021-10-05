namespace RosettaUI
{
    public class WindowLauncherElement : BoolFieldElement
    {
        public WindowLauncherElement(LabelElement label, WindowElement window) :
            base(label, Binder.Create(() => window.enable, v => window.enable = v))
        {
            Window = window;
            Window.enable = false;
        }

        public WindowElement Window { get; }
    }
}