using System.Collections.Generic;

namespace Comugi
{
    public class Dropdown : ValueElement<int>
    {
        readonly IGetter<IEnumerable<string>> options;

        public Dropdown(BinderBase<int> binder, IGetter<IEnumerable<string>> options) : base(binder)
        {
            this.options = options;
        }

        public IEnumerable<string> Options => options.Get();
    }
}