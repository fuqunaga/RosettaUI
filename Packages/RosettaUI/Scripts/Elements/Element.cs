using RosettaUI.Reactive;
using System;

namespace RosettaUI
{
    /// <summary>
    /// RosettaUI's basic unit.
    /// - UI implementation-independent
    /// - Builder creates implementation-dependent UI entities based on Element
    /// - Application accesses UI via Element, does not touch entities
    /// </summary>
    public abstract partial class Element
    {
        #region For Builder

        public readonly ReactiveProperty<bool> enableRx = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<bool> interactableRx = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<Layout> layoutRx = new ReactiveProperty<Layout>();

        public event Action<Element> onDestroy;

        #endregion


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

        public Layout Layout
        {
            get => layoutRx.Value;
            set => layoutRx.Value = value;
        }


        public Element Parent { get; internal set; }


        public virtual void Update()
        {
            if (Enable) UpdateInternal();
        }

        protected virtual void UpdateInternal() { }

        public void Destroy() => onDestroy?.Invoke(this);
    }
}
