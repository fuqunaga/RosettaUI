using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RosettaUI
{
    public abstract class RosettaUIRoot : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        public bool disableKeyboardInputWhileUITyping = true;
        public bool disableMouseOrPointerInputOverUI = true;
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
            
#if ENABLE_INPUT_SYSTEM
            InputDeviceBlocker.shouldBlockInput -= IsMouseOrPointerOverUIInstance;
#endif
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

        public bool IsMouseOrPointerOverUIInstance()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && IsOverUIInstance(mouse.position.ReadValue()))
            {
                return true;
            }
            
            var pointer = Pointer.current;
            if (pointer != null && IsOverUIInstance(pointer.position.ReadValue()))
            {
                return true;
            }
            
            return false;
#else
            return IsOverUIInstance(Input.mousePosition);
#endif
        }

        public abstract bool WillUseKeyInput();
        public abstract bool IsOverUIInstance(Vector2 screenPosition);

        protected abstract void BuildInternal(Element element);


#if ENABLE_INPUT_SYSTEM
        // https://discussions.unity.com/t/prevent-key-input-when-inputfield-has-focus/737128/3
        private void UpdateInputSystem()
        {
            if (disableMouseOrPointerInputOverUI != InputDeviceBlocker.Enabled)
            {
                if(disableMouseOrPointerInputOverUI)
                {
                    InputDeviceBlocker.SetEnable(true);
                    InputDeviceBlocker.shouldBlockInput += IsMouseOrPointerOverUIInstance;
                }
                else
                {
                    InputDeviceBlocker.shouldBlockInput -= IsMouseOrPointerOverUIInstance;
                }
            }

            
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

        private static readonly List<RosettaUIRoot> Roots = new();

        private static void Register(RosettaUIRoot root)
        {
            if(Roots.Contains(root)) return;
            Roots.Add(root);
        }

        private static void Unregister(RosettaUIRoot root)
        {
            Roots.Remove(root);
        }

        public static bool WillUseKeyInputAny()
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var root in Roots)
            {
                if (root.WillUseKeyInput())
                {
                    return true;
                }
            }
            
            return false;
        }

        public static bool IsMouseOrPointerOverUI()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && IsOverUI(mouse.position.ReadValue()))
            {
                return true;
            }
            
            var pointer = Pointer.current;
            if (pointer != null && IsOverUI(pointer.position.ReadValue()))
            {
                return true;
            }
            
            return false;
#else
            return IsOverUI(Input.mousePosition);
#endif
        }

        public static bool IsOverUI(Vector2 screenPosition)
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var root in Roots)
            {
                if ( root.IsOverUIInstance(screenPosition))
                {
                    return true;
                }
            }
            
            return false;
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