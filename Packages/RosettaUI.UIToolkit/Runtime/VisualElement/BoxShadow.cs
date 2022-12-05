using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class BoxShadow : VisualElement
    {
        private const string UssClassName = "rosettaui-box-shadow";

        public BoxShadow()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(UssClassName);
        } 
    }
}