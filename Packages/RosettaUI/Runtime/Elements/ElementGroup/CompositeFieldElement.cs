using System.Collections.Generic;

namespace RosettaUI
{
    /// <summary>
    /// A single line Field that combines multiple Fields
    /// </summary>
    public class CompositeFieldElement : ElementGroupWithHeader
    {
        public readonly LabelElement label;
        
        public CompositeFieldElement(LabelElement label, IEnumerable<Element> contents) : base(label, contents)
        {
            if (label is {labelType: LabelType.Auto})
            {
                label.labelType = LabelType.Prefix;
            }

            this.label = label;
        }
    }
}