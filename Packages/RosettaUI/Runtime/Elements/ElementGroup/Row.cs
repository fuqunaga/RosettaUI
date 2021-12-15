using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public class Row : ElementGroup
    {
        public Row(IEnumerable<Element> children) : base(children)
        {
        }

        public override string DisplayName
        {
            get
            {
                var elem = elements?.FirstOrDefault();
                return elem switch
                {
                    LabelElement label => label,
                    ElementGroup group => group.DisplayName,
                    _ => GetType().Name,
                };
            }
        }
        public override bool IsTreeViewIndentGroup => false;
    }
}