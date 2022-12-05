using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_CompositeField(Element element)
        {
            var compositeField = new CompositeField();
            Bind_CompositeField(element, compositeField);
            return compositeField;
        }

        private bool Bind_CompositeField(Element element, VisualElement visualElement)
        {
            if (element is not CompositeFieldElement compositeFieldElement || visualElement is not CompositeField compositeField) return false;
            
            Bind_ExistingLabel(compositeFieldElement.Label, compositeField.labelElement, str => compositeField.label = str);

            return Bind_ElementGroupContents(compositeFieldElement, compositeField);
        }
    }
}