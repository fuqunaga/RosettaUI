using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RosettaUI
{
    /// <summary>
    /// 子供として任意の Element を持てる Element
    /// コンテンツが静的な場合は IEnumerable<Elements> を直接渡す
    /// </summary>
    public abstract class ElementGroup : Element
    {
        #region For Builder

        public event Action<ElementGroup> onRebuildChildern;

        public void RebuildChildren() => onRebuildChildern?.Invoke(this);

        #endregion


        protected List<Element> _elements;
        public ReadOnlyCollection<Element> Elements => _elements.AsReadOnly();

        public virtual string displayName => GetType().Name;


        protected ElementGroup() { }

        public ElementGroup(IEnumerable<Element> elements)
        {
            SetElements(elements);
        }

        protected void SetElements(IEnumerable<Element> elements)
        {
            _elements = elements.Where(e => e != null).ToList();
            foreach (var e in _elements)
            {
                e.parent = this;
            }
        }

        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (_elements != null)
            {
                foreach (var elem in _elements)
                {
                    elem.Update();
                }
            }
        }
    }
}