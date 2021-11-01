using System.Collections.Generic;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public abstract class OpenCloseBaseElement : ElementGroupWithBar
    { 
        public readonly ReactiveProperty<bool> isOpenRx = new ReactiveProperty<bool>();

        public bool IsOpen
        {
            get => isOpenRx.Value;
            set => isOpenRx.Value = value;
            
        }
        protected OpenCloseBaseElement(Element bar, IEnumerable<Element> contents) :base(bar, contents)
        {
        }
    }
}