using System.Collections.Generic;

namespace RosettaUI
{
    public class DropdownElement : FieldBaseElement<int>
    {
        public readonly IEnumerable<string> options;

        public DropdownElement(LabelElement label, IBinder<int> binder, IEnumerable<string> options) : base(label, binder)
        {
            this.options = options;
        }
    }
}