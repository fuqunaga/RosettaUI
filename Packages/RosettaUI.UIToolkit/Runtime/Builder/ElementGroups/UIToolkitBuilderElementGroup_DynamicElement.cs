using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_DynamicElement(Element element, VisualElement visualElement)
        {
            if (element is not DynamicElement dynamicElement) return false;
            
            dynamicElement.GetViewBridge().RegisterBindView(e => Bind_ElementGroupContents(e, visualElement));

            return true;
        }
    }
}