namespace RosettaUI
{
    public class WindowLauncherElement : BoolFieldElement
    {
        public readonly WindowElement window;
        
        public WindowLauncherElement(LabelElement label, WindowElement window) :
            base(label, Binder.Create(() => window.IsOpen, v => window.IsOpen = v))
        {
            this.window = window;
            this.window.IsOpen = false;
        }
    }
}