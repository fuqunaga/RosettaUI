using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static partial class UI
    {
        public static WindowElement Window(params Element[] elements) => Window(null, elements);

        public static WindowElement Window(LabelElement title, params Element[] elements) => new(title, elements.AsEnumerable());

        public static WindowElement Window(LabelElement title, IEnumerable<Element> elements) => new(title, elements);
    }
}