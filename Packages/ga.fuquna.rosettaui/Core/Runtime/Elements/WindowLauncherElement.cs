﻿namespace RosettaUI
{
    public class WindowLauncherElement : ToggleElement
    {
        public readonly WindowElement window;
        protected override bool ShouldRecordUndo => false;
        
        public WindowLauncherElement(LabelElement label, WindowElement window) :
            base(label, Binder.Create(() => window.IsOpen, v => window.IsOpen = v))
        {
            this.window = window;
            this.window.IsOpen = false;
        }
    }
}