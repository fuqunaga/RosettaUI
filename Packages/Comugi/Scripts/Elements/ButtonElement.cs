using System;

namespace Comugi
{
    public class ButtonElement : ReadOnlyValueElement<string>
    {
        public readonly Action onClick;

        public ButtonElement(IGetter<string> readName = null, Action onClick = null) : base(readName)
        {
            this.onClick = onClick;
        }
    }
}