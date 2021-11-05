using System.Linq;


namespace RosettaUI.Builder
{
    public static class LayoutHint
    {
        public static int GetIndent(this Element element)
        {
            var indent = 0;
            var parent = element.Parent;
            while (parent != null)
            {
                if (parent is IndentElement || parent is FoldElement) indent++;
                parent = parent.Parent;
            }

            return indent;
        }

        public static bool IsLeftMost(this Element element)
        {
            var parent = element.Parent;
            while(parent != null)
            {
                ElementGroup parentGroup = parent switch
                {
                    Row r => r,
                    CompositeFieldElement c => c,
                    _ => null
                };

                if ((parentGroup != null) && (parentGroup.Children.FirstOrDefault() != element))
                {
                    return false;
                }


                element = parent;
                parent = element.Parent;
            }

            return true;
        }
    }
}