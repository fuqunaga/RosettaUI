using System.Collections.Generic;

namespace RosettaUI
{
    public class ScrollViewElement : ElementGroup
    {
        public readonly ScrollViewType type;
        
        public ScrollViewElement(IEnumerable<Element> contents, ScrollViewType type = ScrollViewType.VerticalAndHorizontal) : base(contents)
        {
            this.type = type;
        }
    }
}