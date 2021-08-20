using System;

namespace Comugi
{
    public class BoolField : ValueElement<bool>
    {
        public BoolField(BinderBase<bool> binder) : base(binder) { }
    }
}