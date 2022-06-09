using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_DynamicElement(Element element)
        {
            var ve = new VisualElement();
            ve.AddToClassList(UssClassName.DynamicElement);

            return ve;
        }
    }
}