using System.Linq;


namespace RosettaUI.Builder
{
    public static class LayoutHint
    {
        public static bool IsCompositeFieldLabel(this Element element) => GetAncestorCompositeField(element)?.label == element;

        static CompositeFieldElement GetAncestorCompositeField(Element element)
        {
            Element current = element;
            var parent = current.parent;

            while (parent != null)
            {
                if (parent is CompositeFieldElement compositeField)
                {
                    return compositeField;
                }

                current = parent;
                parent = current.parent;
            }
            return null;
        }


        public static int GetIndent(this Element element)
        {
            var indent = 0;
            var parent = element.parent;
            while (parent != null)
            {
                if (parent is FoldElement) indent++;
                parent = parent.parent;
            }

            return indent;
        }

        public static bool IsLeftMost(this Element element)
        {
            var parent = element.parent;
            while(parent != null)
            {
                ElementGroup parentGroup = parent switch
                {
                    Row r => r,
                    CompositeFieldElement c => c,
                    _ => null
                };

                if ((parentGroup != null) && (parentGroup.Elements.FirstOrDefault() != element))
                {
                    return false;
                }


                element = parent;
                parent = element.parent;
            }

            return true;
        }
    }
}