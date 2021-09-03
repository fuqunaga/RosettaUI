using System.Linq;


namespace RosettaUI.Builder
{
    public static class LayoutHint
    {
        public static bool IsCompositeFieldLabel(this Element element) => GetAncestorCompositeField(element)?.label == element;

        public static bool IsCompositeFieldContents(this Element element) => GetAncestorCompositeField(element) != null;

        static CompositeFieldElement GetAncestorCompositeField(Element element)
        {
            Element current = element;
            var parent = current.parentGroup;

            while (parent != null)
            {
                if (parent is CompositeFieldElement compositeField)
                {
                    return compositeField;
                }

                current = parent;
                parent = current.parentGroup;
            }
            return null;
        }
    }
}