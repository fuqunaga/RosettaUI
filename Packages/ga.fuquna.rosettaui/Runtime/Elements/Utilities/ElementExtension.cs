using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace RosettaUI
{
    public static class ElementExtension
    {
        #region Element
        
        public static IEnumerable<Element> AsEnumerable(this Element element) => new ElementEnumerable(element);

        public static IEnumerable<T> Query<T>(this Element element)
        {
            foreach(var e in element.AsEnumerable())
            {
                if (e is T t) yield return t;
            }
        }
        
        public static bool EnableInHierarchy(this Element element) => element.Parents().All(e => e.Enable);

        public static IEnumerable<Element> Parents(this Element element)
        {
            for (var parent = element.Parent; parent != null; parent = parent.Parent)
            {
                yield return parent;
            }
        }

        public static LabelElement FirstLabel(this Element element) => element.Query<LabelElement>().FirstOrDefault();
        
        
        public static bool ValidateSingleParent(this Element element)
        {
            var invalid = element.Parent != null;
            if ( invalid )
            {
                using var _ = ListPool<string>.Get(out var lineage);
                var target = element;
                while (target != null)
                {
                    var str = target switch
                    {
                        LabelElement l when !string.IsNullOrEmpty(l.Value) => $"Label[{l.Value}]",
                        _ => target.GetType().ToString()
                    };
                        
                    lineage.Add(str);

                    target = target.Parent;
                }

                lineage.Reverse();
                Debug.LogError($"Element already has a parent. {string.Join(" > ", lineage)}.\nAn element can only have a single parent.");
            }

            return invalid;
        }

        
        private readonly struct ElementEnumerable : IEnumerable<Element>
        {
            private readonly Element _element;

            public ElementEnumerable(Element element) => _element = element;
            public IEnumerator<Element> GetEnumerator()
            {
                yield return _element;
                foreach (var child in _element.Children.SelectMany(e => e.AsEnumerable()))
                {
                    yield return child;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        
        #endregion
        
        #region LabelElement

        public static LabelElement Clone(this LabelElement me) => new(me);

        #endregion
    }
}