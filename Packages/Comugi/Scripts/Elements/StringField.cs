using System;

namespace RosettaUI
{
    public class StringField : FieldBase<string>
    {
        public StringField(LabelElement label, BinderBase<string> binder) : base(label, binder) { }
    }
}