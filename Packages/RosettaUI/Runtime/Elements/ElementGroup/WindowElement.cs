using System.Collections.Generic;
using RosettaUI.Reactive;
using UnityEngine;

namespace RosettaUI
{
    public class WindowElement : OpenCloseBaseElement
    {
        public readonly ReactiveProperty<Vector2?> positionRx = new();

        private bool _closable = true;
        
        public Vector2? Position
        {
            set => positionRx.Value = value;
        }

        public bool Closable
        {
            get => _closable;
            set
            {
                if (_closable == value) return;
                if (HasBuilt)
                {
                    Debug.LogWarning($"{nameof(WindowElement)}.{nameof(Closable)} is not applied after {nameof(RosettaUIRoot)}.{nameof(RosettaUIRoot.Build)}().");
                }
                
                _closable = value;
            }
        }

        public override ReactiveProperty<bool> IsOpenRx => enableRx;

        public WindowElement(Element header, IEnumerable<Element> contents) : base(header, contents)
        {
        }
    }
}