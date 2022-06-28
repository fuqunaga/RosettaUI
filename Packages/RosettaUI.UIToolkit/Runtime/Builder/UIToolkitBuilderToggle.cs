using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_Toggle(Element element)
        {
            var toggleElement = (ToggleElement) element;
            var toggle = Build_Field<bool, Toggle>(element, !toggleElement.isLabelRight);
            if (toggleElement.isLabelRight)
            {
                toggleElement.label?.SubscribeValueOnUpdateCallOnce(text => toggle.text = text);
            }

            return toggle;
        }
    }
}