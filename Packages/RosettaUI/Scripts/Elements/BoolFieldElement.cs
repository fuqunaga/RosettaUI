using System;

namespace RosettaUI
{
    public class BoolFieldElement : FieldBaseElement<bool>
    {
        public BoolFieldElement(LabelElement label, BinderBase<bool> binder) : base(label, binder) { }
    }
}