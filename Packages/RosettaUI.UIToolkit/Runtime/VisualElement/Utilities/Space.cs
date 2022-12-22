using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class Space : VisualElement
    {
        private const string UssClassName = "rosettaui-space";

        public Space()
        {
            AddToClassList(UssClassName);
        }
    }
}