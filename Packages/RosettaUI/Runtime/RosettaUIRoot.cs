using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{
    public abstract class RosettaUIRoot : MonoBehaviour
    {
        public readonly ElementUpdater updater = new ElementUpdater();

        private readonly List<Element> _elements = new List<Element>();

        private readonly Queue<Func<Element>> _createElementOnEnableQueue = new Queue<Func<Element>>();
        
        #region Unity

        protected virtual void OnEnable()
        {
            while (_createElementOnEnableQueue.Count > 0)
            {
                var func = _createElementOnEnableQueue.Dequeue();
                Build(func());
            }
            
            Register(this);

            foreach (var element in _elements)
            {
                element.Enable = true;
            }
        }

        protected virtual void OnDisable()
        {
            Unregister(this);
        }

        protected virtual void Update()
        {
            updater.Update();
        }

        protected void OnDestroy()
        {
            foreach(var e in _elements) e.Destroy();
        }

        #endregion

        
        protected abstract void BuildInternal(Element element);

        public abstract bool WillUseKeyInput();

        public void Build(Element element)
        {
            BuildInternal(element);
            
            updater.Register(element);
            updater.RegisterWindowRecursive(element);
            
            _elements.Add(element);
        }

        public void BuildOnEnable(Func<Element> createElement)
        {
            _createElementOnEnableQueue.Enqueue(createElement);
        }

        
        #region static

        private static readonly HashSet<RosettaUIRoot> Roots = new HashSet<RosettaUIRoot>();

        private static void Register(RosettaUIRoot root)
        {
            Roots.Add(root);
        }

        private static void Unregister(RosettaUIRoot root)
        {
            Roots.Remove(root);
        }

        public static bool WillUseKeyInputAny()
        {
            return Roots.Any(r => r.WillUseKeyInput());
        }

        #endregion
    }
}