using System.Collections.Generic;

namespace RosettaUI
{
    public class BoxElement : ElementGroup
    {
        public BoxElement(Element element) : this(new[] { element }) { }

        public BoxElement(IEnumerable<Element> elements) : base(elements) { }
        public override bool IsTreeViewIndentGroup => true;
    }
}