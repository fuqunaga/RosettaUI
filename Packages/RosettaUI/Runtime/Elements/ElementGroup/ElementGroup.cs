using System;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    /// <summary>
    /// 子供として任意の Element を持てる Element
    /// </summary>
    public abstract class ElementGroup : Element
    {
        #region For Builder

        public event Action<ElementGroup> onRebuildChildren;

        protected void RebuildChildren() => onRebuildChildren?.Invoke(this);

        #endregion

        // Children　Update()で更新される子要素すべて
        // Contents　特殊な意味の子要素は含まれない
        // WindowのタイトルバーなどはChildrenに含むがContentsには含まない
        public virtual IEnumerable<Element> Contents => Children;

        public virtual string DisplayName => GetType().Name;

        protected ElementGroup() { }

        public ElementGroup(IEnumerable<Element> elements)
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
    }
}