using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public class RowElement : ElementGroup
    {
        public RowElement(IEnumerable<Element> children) : base(children)
        {
        }
    }
}