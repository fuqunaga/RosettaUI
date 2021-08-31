using System.Collections.Generic;

namespace RosettaUI
{
    public class Dropdown : FieldBaseElement<int>
    {
        public readonly IEnumerable<string> options;

        public Dropdown(LabelElement label, BinderBase<int> binder, IEnumerable<string> options) : base(label, binder)
        {
            this.options = options;
        }
    }
}