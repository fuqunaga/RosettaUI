using System;

namespace RosettaUI
{
    public class StringFieldElement : FieldBaseElement<string>
    {
        public StringFieldElement(LabelElement label, BinderBase<string> binder) : base(label, binder) { }
    }
}