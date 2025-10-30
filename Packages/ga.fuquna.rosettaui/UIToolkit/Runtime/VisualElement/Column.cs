using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class Column : VisualElement
    {
        private const string UssClassName = "rosettaui-column";
        
        public Column()
        {
            AddToClassList(UssClassName);
        }
    }
}