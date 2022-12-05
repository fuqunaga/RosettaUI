using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_ElementGroup<TElementGroup, TVisualElement>(Element element)
            where TElementGroup : ElementGroup
            where TVisualElement : VisualElement, new()
        {
            var visualElement = new TVisualElement();
            Bind_ElementGroup<TElementGroup, TVisualElement>(element, visualElement);
            return visualElement;
        }

        private bool Bind_ElementGroup<TElementGroup, TVisualElement>(Element element, VisualElement visualElement)
            where TElementGroup : ElementGroup
            where TVisualElement : VisualElement
        {
            if (element is not TElementGroup elementGroup || visualElement is not TVisualElement) return false;
            return Bind_ElementGroupContents(elementGroup, visualElement);
        }
    }
}