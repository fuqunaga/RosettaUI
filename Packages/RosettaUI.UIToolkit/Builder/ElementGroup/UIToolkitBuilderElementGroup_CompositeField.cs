using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_CompositeField(Element element)
        {
            var compositeFieldElement = (CompositeFieldElement) element;

            var field = new VisualElement();
            field.AddToClassList(UssClassName.UnityBaseField);
            field.AddToClassList(UssClassName.CompositeField);

            var labelElement = compositeFieldElement.header;
            if (labelElement != null)
            {
                var label = Build(labelElement);
                label.AddToClassList(UssClassName.UnityBaseFieldLabel);
                field.Add(label);
            }

            var contentContainer = new VisualElement();
            contentContainer.AddToClassList(UssClassName.CompositeFieldContents);
            field.Add(contentContainer);
            Build_ElementGroupContents(contentContainer, element);

            return field;
        }
    }
}