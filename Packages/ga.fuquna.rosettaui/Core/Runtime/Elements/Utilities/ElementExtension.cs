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
        
        // 自分と親がすべてEnableかつ最後の親がRootに登録されていたらtrue
        public static bool EnableInHierarchy(this Element element)
        {
            var e = element;
            while (true)
            {
                if (e is not {Enable: true}) return false;
                if (e.Parent == null) return RosettaUIRoot.IsRootElement(e);
                e = e.Parent;
            }
        }

        public static IEnumerable<Element> SelfAndParents(this Element element) => element.Parents().Prepend(element);
        
        public static IEnumerable<Element> Parents(this Element element)
        {
            for (var parent = element.Parent; parent != null; parent = parent.Parent)
            {
                yield return parent;
            }
        }

        public static bool TryGetRootElement(this Element element, out Element rootElement)
        {
            var last = element.SelfAndParents().LastOrDefault();
            var isRoot = RosettaUIRoot.IsRootElement(last);
            rootElement = isRoot ? last : null;
            return isRoot;
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