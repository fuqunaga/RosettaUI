using System.Collections.Generic;
using System.Linq;

namespace Comugi
{
    public class Row : ElementGroup
    {
        public Row(IEnumerable<Element> children) : base(children)
        {
        }

        public override string displayName
        {
            get
            {
                var elem = _elements?.FirstOrDefault();
                return elem switch
                {
                    LabelElement label => label,
                    ElementGroup group => group.displayName,
                    _ => GetType().Name,
                };
            }
        }
    }
}