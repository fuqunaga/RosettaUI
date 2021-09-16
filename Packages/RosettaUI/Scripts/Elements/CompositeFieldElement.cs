using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    /// <summary>
    /// A single line Field that combines multiple Fields
    /// </summary>
    public class CompositeFieldElement : ElementGroup
    {
        public readonly LabelElement label;
        public readonly Element contents;

        public CompositeFieldElement(LabelElement label, Element contents) : base(new Element[] { label, contents })
        {
            this.label = label;
            this.contents = contents;        
        }
    }
}