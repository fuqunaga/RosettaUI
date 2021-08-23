using System;

namespace Comugi
{
    public class BoolField : FieldBase<bool>
    {
        public BoolField(Label label, BinderBase<bool> binder) : base(label, binder) { }
    }
}