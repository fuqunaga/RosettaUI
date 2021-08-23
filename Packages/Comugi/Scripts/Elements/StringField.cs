using System;

namespace Comugi
{
    public class StringField : FieldBase<string>
    {
        public StringField(Label label, BinderBase<string> binder) : base(label, binder) { }
    }
}