using System.Collections.Generic;

namespace RosettaUI
{
    /// <summary>
    /// A single line Field that combines multiple Fields
    /// </summary>
    public class CompositeFieldElement : ElementGroupWithBar, IFieldElement
    {
        public LabelElement Label => bar as LabelElement;
        
        public CompositeFieldElement(LabelElement label, IEnumerable<Element> contents) : base(label, contents)
        {
        }

        public override bool IsTreeViewIndentGroup => false;
    }
}