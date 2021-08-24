using System;

namespace Comugi
{
    public class BoolField : FieldBase<bool>
    {
        public BoolField(LabelElement label, BinderBase<bool> binder) : base(label, binder) { }
    }
}