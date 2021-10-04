using System;

namespace RosettaUI
{
    public class WindowLauncherElement : ButtonElement
    {
        public WindowElement Window { get; }

        public WindowLauncherElement(IGetter<string> readName, WindowElement window) : base(readName)
        {
            Window = window;
        }
    }
}