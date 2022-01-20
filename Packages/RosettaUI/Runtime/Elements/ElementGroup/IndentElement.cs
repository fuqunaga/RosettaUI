using System.Collections.Generic;

namespace RosettaUI
{
    public class IndentElement : ElementGroup
    {
        public readonly int level;
        protected IndentElement() { }

        public IndentElement(IEnumerable<Element> elements, int level = 1) : base(elements)
        {
            this.level = level;
        }
        
        public override bool IsTreeViewIndentGroup => true;
    }
}