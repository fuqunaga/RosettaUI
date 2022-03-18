using System.Collections.Generic;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public abstract class OpenCloseBaseElement : ElementGroupWithHeader
    {
        public abstract ReactiveProperty<bool> IsOpenRx { get; }

        public bool IsOpen
        {
            get => IsOpenRx.Value;
            set => IsOpenRx.Value = value;
        }
        
        protected OpenCloseBaseElement(Element header, IEnumerable<Element> contents) :base(header, contents)
        {
        }
    }
}