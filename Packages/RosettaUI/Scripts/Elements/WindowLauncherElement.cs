namespace RosettaUI
{
    public class WindowLauncherElement : BoolFieldElement
    {
        public WindowLauncherElement(LabelElement label, WindowElement window) :
            base(label, Binder.Create(() => window.Enable, v => window.Enable = v))
        {
            Window = window;
            Window.Enable = false;
        }

        public WindowElement Window { get; }

        public override void Update()
        {
            base.Update();
            if (Window.Enable) Window.Update();
        }
    }
}