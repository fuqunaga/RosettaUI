using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public abstract class ElementGroupWithBar : ElementGroup
    {
        public readonly Element bar;
        public override IEnumerable<Element> Contents => bar != null ? Children.Skip(1) : Children;
        
        protected ElementGroupWithBar(Element bar, IEnumerable<Element> contents)
        {
            this.bar = bar;
            SetElements(new[] { this.bar }.Concat(contents));
        }
    }
}