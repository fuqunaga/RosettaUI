using System;

namespace Comugi
{
    public class StringField : ValueElement<string>
    {
        public StringField(BinderBase<string> binder) : base(binder){ }
    }
}