using System.Collections.Generic;

namespace RosettaUI
{
    public class FoldElement : OpenCloseBaseElement
    {
        public FoldElement(LabelElement title, IEnumerable<Element> contents) : base(title, contents)
        {
        }
    }
}