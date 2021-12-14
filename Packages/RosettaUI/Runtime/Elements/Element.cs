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
        public readonly ReactiveProperty<bool> enableRx = new(true);
        public readonly ReactiveProperty<bool> interactableRx = new(true);

        public bool UpdateWhenDisabled { get; set; }
        
        public event Action<Element> onUpdate;
        public event Action onViewValueChanged;
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

        public Style Style { get; } = new();
        
        public Element Parent { get; private set; }

        public void SetParent(Element element)
        {
            this.ValidateSingleParent();
            Parent = element;
        }


        public virtual void Update()
        {
            if (Enable || UpdateWhenDisabled) UpdateInternal();
        }

        protected virtual void UpdateInternal()
        {
            onUpdate?.Invoke(this);
        }

        public virtual void Destroy() => onDestroy?.Invoke(this);

        protected void NotifyViewValueChanged()
        {
            onViewValueChanged?.Invoke();
            Parent?.NotifyViewValueChanged();
        }
    }
}
