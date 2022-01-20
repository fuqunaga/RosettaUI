using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Builder
{
    public static class LayoutHint
    {
        public static int GetIndentLevel(this Element element)
            => Mathf.Max(0, element.AsIndentParentEnumerable().Sum(pair =>
            {
                var (currentElement, parent) = pair;
                
                return parent switch
                {
                    IndentElement indentElement => indentElement.level,
                    FoldElement foldElement => foldElement.bar != currentElement ? -1 : 0,
                    _ => 0
                };
            }));
        

        public static bool IsLeftMost(this Element element)
        {
            foreach (var (currentElement, parent) in element.AsIndentParentEnumerable())
            {
                switch (parent)
                {
                    case RowElement row when row.Children.FirstOrDefault() != currentElement:
                        return false;

                    case CompositeFieldElement c:
                    {
                        if (c.Children.FirstOrDefault() != currentElement)
                        {
                            return false;
                        }

                        break;
                    }
                }
            }

            return true;
        }
    }


    static class ElementIndentParentEnumerableExtension
    {
        public static IEnumerable<(Element, Element)> AsIndentParentEnumerable(this Element element) =>
            new ElementIndentParentEnumerable(element);


        readonly struct ElementIndentParentEnumerable : IEnumerable<(Element, Element)>
        {
            private readonly Element _element;

            public ElementIndentParentEnumerable(Element element) => _element = element;

            public IEnumerator<(Element,Element)> GetEnumerator()
            {
                var element = _element;
                var parent = element.Parent;
                for (;
                     parent != null && parent is not PageElement;
                     element = parent, parent = element.Parent)
                {
                    yield return (element, parent);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}