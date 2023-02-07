using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RosettaUI
{
    public abstract class RosettaUIRoot : MonoBehaviour
    {
        public readonly ElementUpdater updater = new();

        private readonly List<Element> _elements = new();
        public IReadOnlyList<Element> Elements => _elements;

        private readonly Queue<Func<Element>> _createElementOnEnableQueue = new();


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
            foreach (var e in _elements) e.DetachView();
        }


        // suppress Input trick
        protected virtual void OnGUI()
        {
            SuppressInputKey();
        }

        #endregion

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

        public abstract bool WillUseKeyInput();

        protected abstract void BuildInternal(Element element);


        private static PropertyInfo _textFieldInputPropertyInfo;
        
        protected void SuppressInputKey()
        {
            _textFieldInputPropertyInfo ??= typeof(GUIUtility).GetProperty("textFieldInput", BindingFlags.NonPublic | BindingFlags.Static);

            if (WillUseKeyInput())
            {
                _textFieldInputPropertyInfo.SetValue(null, true);
            }
        }

        
        #region Static

        private static readonly HashSet<RosettaUIRoot> Roots = new();

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

        public static void GlobalBuild(Element element)
        {
            var root = Roots.FirstOrDefault();
            if (root == null)
            {
                Debug.LogWarning($"There is no active {nameof(RosettaUIRoot)}.");
                return;
            }
            
            root.Build(element);
        }

        #endregion
    }
}