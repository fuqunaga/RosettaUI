using System;

namespace RosettaUI
{
    public class StringFieldElement : FieldBaseElement<string>
    {
        public StringFieldElement(LabelElement label, IBinder<string> binder) : base(label, binder) { }
    }
}