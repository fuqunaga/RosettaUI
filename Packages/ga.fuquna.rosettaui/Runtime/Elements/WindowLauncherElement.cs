namespace RosettaUI
{
    public class WindowLauncherElement : ToggleElement
    {
        public readonly WindowElement window;
        
        public WindowLauncherElement(LabelElement label, WindowElement window) :
            base(label, Binder.Create(() => window.IsOpen, v => window.IsOpen = v))
        {
            ShouldRecordUndo = false;
            
            this.window = window;
            this.window.IsOpen = false;
        }
    }
}