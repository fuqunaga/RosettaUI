using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public abstract class ElementGroupWithHeader : ElementGroup
    {
        public readonly Element header;
        public override IEnumerable<Element> Contents => header != null ? Children.Skip(1) : Children;
        
        protected ElementGroupWithHeader(Element header, IEnumerable<Element> contents)
        {
            this.header = header;
            
            var children = new[] {header}.AsEnumerable();
            if (contents != null)
            {
                children = children.Concat(contents);
            }

            SetElements(children);
        }
    }
}