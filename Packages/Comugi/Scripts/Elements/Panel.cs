using System.Collections.Generic;

namespace RosettaUI
{
    public class Panel : ElementGroup
    {
        public Panel(Element element) : this(new[] { element }) { }

        public Panel(IEnumerable<Element> elements) : base(elements) { }
    }
}