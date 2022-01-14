using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Builder
{
    public static class LayoutHint
    {
        public static bool IsTreeViewIndentTarget(this Element element)
        {
            return IsValidElement(element) && IsValidParent(element) && element.IsLeftMost();

            static bool IsValidElement(Element e) =>
                e is not ElementGroup or CompositeFieldElement or FoldElement or BoxElement;

            static bool IsValidParent(Element e) => 
                e.Parent is ElementGroup {IsTreeViewIndentGroup : true} group && group.Contents.Contains(e);

        }
        
        public static int GetIndentLevel(this Element element)
        {
            var indent = element.AsIndentParentEnumerable().Count(parent => parent is IndentElement or FoldElement or BoxElement);
                       
            // Cancelling the  indent of icons width
            if (element is FoldElement)
            {
                indent = Mathf.Max(0, indent - 1);
            }
            
            return indent;
        }
        
        
        /// <summary>
        /// Indent value from nearest indent ancestor
        /// </summary>
        public static int GetParentIndentLevel(this Element element)
        {
            if (!element.IsTreeViewIndentTarget()) return 0;

            return element.AsIndentParentEnumerable().FirstOrDefault(IsTreeViewIndentTarget)?.GetIndentLevel() ?? 0;
        }

        public static bool IsLeftMost(this Element element)
        {
            foreach(var parent in element.AsIndentParentEnumerable())
            {
                switch (parent)
                {
                    case RowElement row when row.Children.FirstOrDefault() != element:
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
            }
            
            return true;
        }
    }


    static class ElementIndentParentExtension
    {
        public static IEnumerable<Element> AsIndentParentEnumerable(this Element element) =>
            new ElementIndentParentEnumerable(element);
        
        
        readonly struct ElementIndentParentEnumerable : IEnumerable<Element>
        {
            private readonly Element _element;

            public ElementIndentParentEnumerable(Element element) => _element = element;

            public IEnumerator<Element> GetEnumerator()
            {
                for (var parent = _element.Parent; parent != null && parent is not PageElement; parent = parent.Parent)
                    yield return parent;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }


}