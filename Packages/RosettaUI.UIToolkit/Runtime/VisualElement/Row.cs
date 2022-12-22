using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class Row : VisualElement
    {
        private const string UssClassName = "rosettaui-row";
        
        public Row()
        {
            AddToClassList(UssClassName);
        }
    }
}