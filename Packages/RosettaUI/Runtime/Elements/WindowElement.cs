using System.Collections.Generic;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public class WindowElement : OpenCloseBaseElement
    {
        public WindowElement(Element header, IEnumerable<Element> contents) : base(header, contents)
        {}

        public override ReactiveProperty<bool> IsOpenRx => enableRx;
    }
}