using RosettaUI.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;

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
        
        public void Destroy(bool isDestroyRoot = true)
        {
            onDestroy?.Invoke(this, isDestroyRoot);
            DestroyChildren(false);

            // Element.Childrenは追加削除できない親と一体化した要素も含む
            // 追加削除できるものはElementGroupのContentsなので削除
            if (Parent is ElementGroup)
            {
                Parent.RemoveChild(this);
            }
        }

        public void DestroyChildren(bool isDestroyRoot = true)
        {
            var node = _children.First;
            while(node != null)
            {
                var next = node.Next;
                node.Value.Destroy(isDestroyRoot);
                node = next;
            }
        }

        public void NotifyViewValueChanged()
        {
            onViewValueChanged?.Invoke();
            Parent?.NotifyViewValueChanged();
        }
    }
}
