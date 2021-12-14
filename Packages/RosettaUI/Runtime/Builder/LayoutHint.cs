using System.Linq;


namespace RosettaUI.Builder
{
    public static class LayoutHint
    {
        public static int GetIndentLevel(this Element element)
        {
            var indent = 0;
            var parent = element.Parent;
            while (parent != null)
            {
                if (parent is IndentElement or FoldElement) indent++;
                parent = parent.Parent;
            }

            return indent;
        }

        public static bool IsLeftMost(this Element element)
        {
            var parent = element.Parent;
            while(parent != null)
            {
                switch (parent)
                {
                    case Row row when row.Children.FirstOrDefault() != element:
                        return false;

                    case CompositeFieldElement c when c.Children.FirstOrDefault() != element:
                        return false;
                    
                    case ElementGroupWithBar eb when eb.bar == element:
                        return false;
                }

                element = parent;
                parent = element.Parent;
            }

            return true;
        }
    }
}