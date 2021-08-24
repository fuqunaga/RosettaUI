using System;

namespace Comugi
{
    public class StringField : FieldBase<string>
    {
        public StringField(LabelElement label, BinderBase<string> binder) : base(label, binder) { }
    }
}