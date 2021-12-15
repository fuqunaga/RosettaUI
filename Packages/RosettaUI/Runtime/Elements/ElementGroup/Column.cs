using System.Collections.Generic;

namespace RosettaUI
{
    public class Column : ElementGroup
    {
        public Column(IEnumerable<Element> children) : base(children)
        {
        }

        public override bool IsTreeViewIndentGroup => true;
    }
}