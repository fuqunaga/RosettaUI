using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    /// <summary>
    /// 子供として任意の Element を持てる Element
    /// </summary>
    public abstract class ElementGroup : Element
    {
        // Children　Update()で更新される子要素すべて
        // Contents　特殊な意味の子要素は含まれない
        // WindowのタイトルバーなどはChildrenに含むがContentsには含まない
        public virtual IEnumerable<Element> Contents => Children;

        public virtual string DisplayName => GetType().Name;

        protected ElementGroup() { }

        protected ElementGroup(IEnumerable<Element> elements)
        {
            SetElements(elements);
        }

        protected void SetElements(IEnumerable<Element> elements)
        {
            if (elements != null)
            {
                foreach (var e in elements.Where(e => e != null))
                {
                    AddChild(e);
                }
            }
        }
        
        public Element GetContentAt(int index)
        {
            var i = 0;
            foreach (var c in Contents)
            {
                if (i == index) return c;
                i++;
            }

            return null;
        }
    }
}