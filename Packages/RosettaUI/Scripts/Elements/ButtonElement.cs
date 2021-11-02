using System;

namespace RosettaUI
{
    public class ButtonElement : ReadOnlyValueElement<string>
    {
        public  Action OnClick { get; protected set; }
        
        public ButtonElement(IGetter<string> readName = null, Action onClick = null) : base(readName)
        {
            OnClick = onClick;
        }
    }
}