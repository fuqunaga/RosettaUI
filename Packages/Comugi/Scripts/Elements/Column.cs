using System.Collections.Generic;
using System.Linq;

namespace Comugi
{
    public class Column : ElementGroup
    {
        // Row wrap single element for indent layout
        public Column(IEnumerable<Element> children) :
            base(children
                .Where(e => e != null)
                .Select(e => e as ElementGroup ?? new Row(new[] { e }))
                )
        {
        }
    }
}