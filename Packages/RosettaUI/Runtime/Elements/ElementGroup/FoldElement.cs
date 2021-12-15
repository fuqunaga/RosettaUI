using System.Collections.Generic;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public class FoldElement : OpenCloseBaseElement
    {
        public FoldElement(Element bar, IEnumerable<Element> contents) : base(bar, contents)
        {
        }

        public override ReactiveProperty<bool> IsOpenRx { get; } = new();
    }
}