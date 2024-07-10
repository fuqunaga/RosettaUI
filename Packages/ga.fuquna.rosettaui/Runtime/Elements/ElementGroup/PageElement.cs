using System.Collections.Generic;

namespace RosettaUI
{
    // Labelのインデントを揃えるグループ
    public class PageElement : ColumnElement
    {
        public PageElement(IEnumerable<Element> elements) : base(elements)
        {
        }
    }
}