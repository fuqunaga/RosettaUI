using System.Collections.Generic;

namespace Comugi
{
    public class Panel : ElementGroup
    {
        public Panel(Element element) : this(new[] { element }) { }

        public Panel(IEnumerable<Element> elements) : base(elements) { }
    }
}