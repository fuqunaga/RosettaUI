using System.Collections.Generic;
using RosettaUI.Reactive;
using UnityEngine;

namespace RosettaUI
{
    public class WindowElement : OpenCloseBaseElement
    {
        public readonly ReactiveProperty<Vector2?> positionRx = new();

        public Vector2? Position
        {
            set => positionRx.Value = value;
        }


        public WindowElement(Element header, IEnumerable<Element> contents) : base(header, contents)
        {}

        public override ReactiveProperty<bool> IsOpenRx => enableRx;
    }
}