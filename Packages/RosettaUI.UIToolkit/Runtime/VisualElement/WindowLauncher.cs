using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class WindowLauncher : Toggle
    {
        private const string UssClassName = "rosettaui-window-launcher";
        
        public WindowLauncher()
        {
            AddToClassList(UssClassName);
        }
    }
}