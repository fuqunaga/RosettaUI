using System.Collections.Generic;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public abstract class OpenCloseBaseElement : ElementGroupWithBar
    {
        public abstract ReactiveProperty<bool> IsOpenRx { get; }

        public bool IsOpen
        {
            get => IsOpenRx.Value;
            set => IsOpenRx.Value = value;
        }
        
        protected OpenCloseBaseElement(Element bar, IEnumerable<Element> contents) :base(bar, contents)
        {
        }
    }
}