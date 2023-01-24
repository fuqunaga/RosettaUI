using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_Indent(Element element, VisualElement visualElement)
        {
            if (element is not IndentElement indentElement || visualElement is not Indent indent) return false;
            
            ApplyIndent(indent, indentElement.level);

            return Bind_ElementGroupContents(indentElement, indent);
        }
    }
}