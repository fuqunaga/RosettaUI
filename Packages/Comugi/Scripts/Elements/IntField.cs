using System;

namespace Comugi
{
    public class IntField : FieldBase<int>
    {
        public IntField(Label label, BinderBase<int> binder) : base(label, binder) { }
    }
}