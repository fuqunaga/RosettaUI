using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class Indent : VisualElement
    {
        private const string UssClassName = "rosettaui-indent";
        
        public Indent()
        {
            AddToClassList(UssClassName);
        }
    }
}