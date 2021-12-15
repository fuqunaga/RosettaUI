using System.Collections.Generic;

namespace RosettaUI
{
    public class WindowElement : ElementGroupWithBar
    {
        public WindowElement(Element bar, IEnumerable<Element> contents) : base(bar, contents)
        {}
        
        public override bool IsTreeViewIndentGroup => true;
    }
}