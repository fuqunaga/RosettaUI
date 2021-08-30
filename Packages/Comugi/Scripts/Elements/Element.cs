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

        public event Action<Element> onRebuild;
        public event Action<Element> onDestroy;

        #endregion


        public bool enable
        {
            get => enableRx.Value;
            set => enableRx.Value = value;
        }

        public bool interactable
        {
            get => interactableRx.Value;
            set => interactableRx.Value = value;
        }

        public Layout layout
        {
            get => layoutRx.Value;
            set => layoutRx.Value = value;
        }


        public ElementGroup parentGroup { get; internal set; }


        public virtual void Update()
        {
            if (enable) UpdateInternal();
        }

        protected virtual void UpdateInternal() { }

        public void Rebuild() => onRebuild?.Invoke(this);
        public void Destroy() => onDestroy?.Invoke(this);
    }
}
