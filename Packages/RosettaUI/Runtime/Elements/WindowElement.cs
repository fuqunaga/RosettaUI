using System.Collections.Generic;

namespace RosettaUI
{
    public class WindowElement : ElementGroupWithHeader
    {
        public WindowElement(Element header, IEnumerable<Element> contents) : base(header, contents)
        {}
    }
}