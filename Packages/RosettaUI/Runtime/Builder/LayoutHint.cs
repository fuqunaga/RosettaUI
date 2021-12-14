using System;
using System.Linq;
using UnityEngine;


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

            // Cancelling the  indent of icons width
            if (element is FoldElement)
            {
                indent = Mathf.Max(0, indent - 1);
            }
            return indent;
        }


        public static bool IsTreeViewIndentTarget(this Element element)
        {
            return IsValidElement(element) && IsValidParent(element) && element.IsLeftMost();
            
            static bool IsValidElement(Element e) => e is not ElementGroup or Row or CompositeFieldElement or FoldElement;
            static bool IsValidParent(Element e) => e.Parent switch
            {
                Column => true,
                IndentElement => true,
                FoldElement f => f.Contents.Contains(e),
                DynamicElement d => IsValidParent(d.Parent),
                _ => false
            };
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

                    case CompositeFieldElement c:
                    {
                        if (c.Children.FirstOrDefault() != element)
                        {
                            return false;
                        }
                        break;
                    }
                }

                element = parent;
                parent = element.Parent;
            }

            return true;
        }
    }
}