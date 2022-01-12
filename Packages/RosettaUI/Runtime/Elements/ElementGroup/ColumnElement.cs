using System.Collections.Generic;

namespace RosettaUI
{
    public class ColumnElement : ElementGroup
    {
        public ColumnElement(IEnumerable<Element> children) : base(children)
        {
        }

        public override bool IsTreeViewIndentGroup => true;
    }
}