using System.Collections.Generic;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public class FoldElement : OpenCloseBaseElement
    {
        public FoldElement(Element header, IEnumerable<Element> contents) : base(header, contents)
        {
        }

        public override ReactiveProperty<bool> IsOpenRx { get; } = new();
    }
}