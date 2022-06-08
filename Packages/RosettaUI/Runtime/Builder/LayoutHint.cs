using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI.Builder
{
    public static class LayoutHint
    {
        public static bool IsIndentOrigin(Element element) => element is PageElement or WindowElement;


        /// <summary>
        /// 子のマイナスインデントを許容しないElement
        /// マイナスインデントは親の左側をはみ出るので Box や Fold では絵的にまずい
        /// </summary>
        public static bool IsIndentWall(Element element)
            => element is BoxElement or FoldElement or TabsElement or ScrollViewElement|| IsIndentOrigin(element);
        
        public static bool CanMinusIndent(this Element element)
        {
            for (var e = element.Parent;
                 e != null && !IsIndentWall(e);
                 e = e.Parent)
            {
                if (e is IndentElement) return true;
            }

            return false;
        }
        

        public static bool IsLeftMost(this Element element)
        {
            foreach (var current in element.AsIndentEnumerable())
            {
                switch (current.Parent)
                {
                    case RowElement row when row.Children.FirstOrDefault() != current:
                        return false;

                    case CompositeFieldElement c:
                        return c.Children.FirstOrDefault() == current;
                }
            }

            return true;
        }
    }


    public static class ElementIndentEnumerableExtension
    {
        public static IEnumerable<Element> AsIndentEnumerable(this Element element) =>
            new ElementIndentParentEnumerable(element);


        readonly struct ElementIndentParentEnumerable : IEnumerable<Element>
        {
            private readonly Element _element;

            public ElementIndentParentEnumerable(Element element) => _element = element;

            public IEnumerator<Element> GetEnumerator()
            {
                
                for (var element = _element;
                     element != null && !LayoutHint.IsIndentOrigin(element);
                     element = element.Parent)
                {
                    yield return element;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}