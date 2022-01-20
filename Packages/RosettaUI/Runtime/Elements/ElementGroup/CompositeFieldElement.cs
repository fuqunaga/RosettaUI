using System.Collections.Generic;

namespace RosettaUI
{
    /// <summary>
    /// A single line Field that combines multiple Fields
    /// </summary>
    public class CompositeFieldElement : ElementGroupWithBar
    {
        public LabelElement Label => bar as LabelElement;
        
        public CompositeFieldElement(LabelElement label, IEnumerable<Element> contents) : base(label, contents)
        {
            label.isPrefix = true;
        }

        public override bool IsTreeViewIndentGroup => false;
    }
}