using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{
    /// <summary>
    /// 任意のElementを子供に持てるElement
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
            if (elements == null) return;
            
            foreach (var e in elements.Where(e => e != null))
            {
                if (e is WindowElement window)
                {
                    Debug.LogWarning($"WindowElement(FirstLabel[{window.FirstLabel()?.Value}]) cannot be a child of another Element. Please using UI.WindowLauncher() or RosettaUIRoot.Build() directly.");
                    continue;
                }
                
                AddChild(e);
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