using System.Collections.Generic;

namespace RosettaUI
{
    /// <summary>
    /// A single line Field that combines multiple Fields
    /// </summary>
    public class CompositeFieldElement : ElementGroupWithBar
    {
        public CompositeFieldElement(Element label, IEnumerable<Element> contents) : base(label, contents)
        {
        }
    }
}