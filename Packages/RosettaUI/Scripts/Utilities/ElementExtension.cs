using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static class ElementExtension
    {
        public static IEnumerable<Element> AsEnumerable(this Element element) => new ElementEnumerable(element);

        public static IEnumerable<T> Query<T>(this Element element)
            where T : Element
        {
            foreach(var e in element.AsEnumerable())
            {
                if (e is T t) yield return t;
            }
        }

        public static LabelElement FirstLabel(this Element element) => element.Query<LabelElement>().FirstOrDefault();

        
        private struct ElementEnumerable : IEnumerable<Element>
        {
            private readonly Element _element;

            public ElementEnumerable(Element element) => _element = element;
            public IEnumerator<Element> GetEnumerator()
            {
                yield return _element;
                while (_element is ElementGroup group)
                {
                    foreach (var child in group.Children.SelectMany(e => e.AsEnumerable()))
                    {
                        yield return child;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}