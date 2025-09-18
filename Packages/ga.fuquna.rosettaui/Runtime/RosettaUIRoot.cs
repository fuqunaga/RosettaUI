using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace RosettaUI
{
#if ENABLE_INPUT_SYSTEM && ! ENABLE_LEGACY_INPUT_MANAGER
    [RequireComponent(typeof(InputSystemUIInputModule))]
#endif
    public abstract class RosettaUIRoot : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        public bool disableKeyboardInputWhileUITyping = true;
#endif
        
        public readonly ElementUpdater updater = new();

        private readonly List<Element> _elements = new();
        private readonly List<Element> _syncEnableElements = new();
        public IReadOnlyList<Element> Elements => _elements;

        private readonly Queue<(Func<Element>,bool)> _createElementOnEnableQueue = new();


        #region Unity

        protected virtual void OnEnable()
        {
            while (_createElementOnEnableQueue.Count > 0)
            {
                var (func, setEnableWhenRootEnabled) = _createElementOnEnableQueue.Dequeue();
                Build(func(), setEnableWhenRootEnabled);
            }

            Register(this);

            foreach (var element in _syncEnableElements)
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
            
#if ENABLE_INPUT_SYSTEM
            UpdateInputSystem();
#endif
        }

        protected void OnDestroy()
        {
            foreach (var e in _elements) e.DetachView();
        }

        #endregion
        

        public void Build(Element element, bool setEnableWhenRootEnabled = true)
        {
            BuildInternal(element);

            updater.Register(element);
            updater.RegisterWindowRecursive(element);

            _elements.Add(element);
            if (setEnableWhenRootEnabled)
            {
                _syncEnableElements.Add(element);
            }
        }

        public void BuildOnEnable(Func<Element> createElement, bool setEnableWhenRootEnabled = true)
        {
            _createElementOnEnableQueue.Enqueue((createElement, setEnableWhenRootEnabled));
        }

        public abstract bool WillUseKeyInput();

        protected abstract void BuildInternal(Element element);

#if ENABLE_INPUT_SYSTEM
        // https://discussions.unity.com/t/prevent-key-input-when-inputfield-has-focus/737128/3
        private void UpdateInputSystem()
        {
            if (!disableKeyboardInputWhileUITyping) return;
            
            var keyboard = Keyboard.current;
            if (WillUseKeyInput() == keyboard.enabled)
            {
                if (keyboard.enabled)
                    InputSystem.DisableDevice(keyboard);
                else
                    InputSystem.EnableDevice(keyboard);
            }
        }
#endif
        
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

        public static void GlobalBuild(Element element, bool setEnableWhenRootEnabled = false)
        {
            var root = Roots.FirstOrDefault();
            if (root == null)
            {
                Debug.LogWarning($"There is no active {nameof(RosettaUIRoot)}.");
                return;
            }
            
            root.Build(element, setEnableWhenRootEnabled);
        }

        #endregion
    }
}