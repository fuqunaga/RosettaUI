using System;
using System.Collections.Generic;
using RosettaUI.Reactive;
using UnityEngine.Assertions;

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
        private readonly LinkedList<Element> _children = new();
        public IEnumerable<Element> Children => _children;


        public readonly ReactiveProperty<bool> enableRx = new(true);
        public readonly ReactiveProperty<bool> interactableRx = new(true);

        public bool UpdateWhileDisabled { get; set; }

        public bool HasBuilt => ViewBridge.HasBuilt;
        
        public event Action<Element> onUpdate;
        public event Action onViewValueChanged;
        public event Action<Element, bool> onDetachView;

        private ElementViewBridge _viewBridge;

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

        private void SetParent(Element element)
        {
            this.ValidateSingleParent();
            Parent = element;
        }

        public bool DetachParent()
        {
            Assert.IsFalse(HasBuilt, $"{GetType()} has already built. Call DestroyView() to reuse this element");
            if (Parent == null) return false;
            Parent.RemoveChild(this);

            return true;
        }

        protected void AddChild(Element element)
        {
            _children.AddLast(element);
            element.SetParent(this);
        }

        protected void RemoveChild(Element element)
        {
            element.Parent = null;
            _children.Remove(element);
        }

        public virtual void Update()
        {
            if (Enable || UpdateWhileDisabled) UpdateInternal();
        }

        protected virtual void UpdateInternal()
        {
            onUpdate?.Invoke(this);
            foreach(var e in _children) e.Update();
        }
        
        public void DetachView(bool destroyView = true)
        {
            onDetachView?.Invoke(this, destroyView);
            DetachViewChildren();

            if (destroyView)
            {
                DetachParent();
            }
        }

        private void DetachViewChildren()
        {
            var node = _children.First;
            while(node != null)
            {
                var next = node.Next;
                node.Value.DetachView(false);
                node = next;
            }
        }

        public void NotifyViewValueChanged()
        {
            onViewValueChanged?.Invoke();
            Parent?.NotifyViewValueChanged();
        }
        
        
        /// <summary>
        /// Interface for UI library
        /// </summary>
        internal ElementViewBridge ViewBridge => _viewBridge ??= CreateViewBridge();

        protected virtual ElementViewBridge CreateViewBridge() => new(this);

        public class ElementViewBridge
        {
            public event Action onUnsubscribe;
            protected readonly Element element;

            public bool HasBuilt => onUnsubscribe != null;

            public ElementViewBridge(Element element) => this.element = element;

            public virtual void UnsubscribeAll()
            {
                onUnsubscribe?.Invoke();
                onUnsubscribe = null;
            } 
        }
    }
    
    public static partial class ElementViewBridgeExtensions
    {
        public static Element.ElementViewBridge GetViewBridge(this Element element) => element.ViewBridge;
    }
}
