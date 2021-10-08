using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public abstract class ElementGroupWithTitle : ElementGroup
    {
        public readonly LabelElement title;
        public override IEnumerable<Element> Contents => title != null ? Children.Skip(1) : Children;
        
        protected ElementGroupWithTitle(LabelElement title, IEnumerable<Element> contents)
        {
            this.title = title;
            SetElements(new Element[] { this.title }.Concat(contents));
        }
    }
}