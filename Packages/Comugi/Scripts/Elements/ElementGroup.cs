using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Comugi
{
    /// <summary>
    /// 子供として複数の Element を持てる Element
    /// リストの要素が増減した場合など動的にリビルドするときはコンストラクタで build function を登録する
    /// コンテンツが静的な場合は IEnumerable<Elements> を直接渡す
    /// </summary>
    public abstract class ElementGroup : Element
    {
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
                e.parentGroup = this;
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