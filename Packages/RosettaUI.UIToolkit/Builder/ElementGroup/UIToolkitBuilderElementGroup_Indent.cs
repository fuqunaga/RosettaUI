using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        VisualElement Build_Indent(Element element)
        {
            var indentElement = (IndentElement) element;

            var ve = new VisualElement();
            ApplyIndent(ve, indentElement.level);

            return Build_ElementGroupContents(ve, element);
        }
    }
}