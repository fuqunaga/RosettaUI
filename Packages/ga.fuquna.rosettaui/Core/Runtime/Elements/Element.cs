﻿using System;
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
        private readonly List<Element> _children = new();
        public IReadOnlyList<Element> Children => _children;

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

        public Element()
        {
            // 可視化したとき表示前にUpdate()が呼ばれていることを保証する
            // onUpdateとかでサイズ調整しているやつを考慮
            enableRx.Subscribe(flag =>
            {
                if (flag && HasBuilt) Update();
            });
        }

        private void SetParent(Element element)
        {
            this.ValidateSingleParent();
            Parent = element;
        }

        public void DetachParent()
        {
            Assert.IsFalse(HasBuilt, $"{GetType()} has already built. Call DestroyView() to reuse this element");
            Parent?.RemoveChild(this, false);
        }

        protected void AddChild(Element element)
        {
            _children.Add(element);
            element.SetParent(this);
        }

        protected bool RemoveChild(Element element, bool destroyView = true)
        {
            if (!_children.Remove(element)) return false;
            element.Parent = null;
            element.DetachView(destroyView);

            return true;
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
            foreach(var child in _children)
            {
                child.DetachView(false);
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
