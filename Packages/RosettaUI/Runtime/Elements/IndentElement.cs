using System.Collections.Generic;

namespace RosettaUI
{
    public class IndentElement : ElementGroup
    {
        protected IndentElement() { }

        public IndentElement(IEnumerable<Element> elements) : base(elements)
        {
        }
    }
}