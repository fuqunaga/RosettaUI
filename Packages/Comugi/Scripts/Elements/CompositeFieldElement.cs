using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{

    public class CompositeFieldElement : ElementGroup
    {
        public readonly LabelElement label;
        public readonly ElementGroup contents;

        public CompositeFieldElement(LabelElement label, ElementGroup contents) : base(new Element[] { label, contents })
        {
            this.label = label;
            this.contents = contents;        
        }
    }
}