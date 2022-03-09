using RosettaUI.Reactive;
using System;
using System.Collections.Generic;

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
        private readonly List<Element> _children = new();
        public IReadOnlyList<Element> Children => _children;


        public readonly ReactiveProperty<bool> enableRx = new(true);
        public readonly ReactiveProperty<bool> interactableRx = new(true);

        public bool UpdateWhileDisabled { get; set; }
        
        public event Action<Element> onUpdate;
        public event Action onViewValueChanged;
        public event Action<Element, bool> onDestroy;


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

        protected void AddChild(Element element)
        {
            _children.Add(element);
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

        protected void DestroyChildren(bool isDestroyRoot)
        {
            foreach (var child in _children)
            {
                child.Parent = null;
                child.Destroy(isDestroyRoot);
            }
            _children.Clear();
        }
        
        public virtual void Destroy(bool isDestroyRoot = true)
        {
            onDestroy?.Invoke(this, isDestroyRoot);
            Parent?.RemoveChild(this);
            
            DestroyChildren(false);
        }

        protected void NotifyViewValueChanged()
        {
            onViewValueChanged?.Invoke();
            Parent?.NotifyViewValueChanged();
        }
    }
}
