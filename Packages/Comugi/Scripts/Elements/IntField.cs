using System;

namespace RosettaUI
{
    public class IntField : FieldBase<int>
    {
        public IntField(LabelElement label, BinderBase<int> binder) : base(label, binder) { }
    }
}