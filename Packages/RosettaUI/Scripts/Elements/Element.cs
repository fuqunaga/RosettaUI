using RosettaUI.Reactive;
using System;

namespace RosettaUI
{
    /// <summary>
    /// RosettaUI basic unit.
    /// - UI implementation-independent
    /// - Builder creates implementation-dependent UI entities based on Element
    /// - Application accesses UI via Element, does not touch entities
    /// </summary>
    public abstract class Element
    {
        public readonly ReactiveProperty<bool> enableRx = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<bool> interactableRx = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<Style> styleRx = new ReactiveProperty<Style>(new Style());

        public bool UpdateWhenDisabled { get; set; }
        
        public event Action<Element> onUpdate;
        public event Action<Element> onDestroy;


        public bool Enable
        {
            get => enableRx.Value;
            set => enableRx.Value = value;
        }

        public bool Interactable
        {
            get => interactableRx.Value;
            set => interactableRx.Value = value;
        }

        public Style Style
        {
            get => styleRx.Value;
            set => styleRx.Value = value;
        }


        public Element Parent { get; internal set; }


        public virtual void Update()
        {
            if (Enable || UpdateWhenDisabled) UpdateInternal();
        }

        protected virtual void UpdateInternal()
        {
            onUpdate?.Invoke(this);
        }

        public virtual void Destroy() => onDestroy?.Invoke(this);
    }
}
