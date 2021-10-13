using System;

namespace RosettaUI
{
    public class BoolFieldElement : FieldBaseElement<bool>
    {
        public BoolFieldElement(LabelElement label, IBinder<bool> binder) : base(label, binder) { }
    }
}