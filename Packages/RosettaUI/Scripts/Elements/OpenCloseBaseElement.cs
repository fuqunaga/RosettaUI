using System.Collections.Generic;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public abstract class OpenCloseBaseElement : ElementGroupWithTitle
    { 
        public readonly ReactiveProperty<bool> isOpenRx = new ReactiveProperty<bool>();

        public bool IsOpen
        {
            get => isOpenRx.Value;
            set => isOpenRx.Value = value;
            
        }
        protected OpenCloseBaseElement(LabelElement title, IEnumerable<Element> contents) :base(title, contents)
        {
        }
    }
}