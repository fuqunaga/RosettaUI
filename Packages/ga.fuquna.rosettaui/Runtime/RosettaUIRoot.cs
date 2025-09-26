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
        public bool disablePointerInputOverUI = true;
        public bool disableMouseInputOverUI = true;
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

#if ENABLE_INPUT_SYSTEM
            RegisterForInputDeviceBlocker();
#endif
        }

        protected virtual void OnDisable()
        {
            Unregister(this);
            
#if ENABLE_INPUT_SYSTEM
            UnregisterForInputDeviceBlocker();
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

        public abstract bool WillUseKeyInput();
        public abstract bool IsOverUIInstance(Vector2 screenPosition);

        protected abstract void BuildInternal(Element element);


#if ENABLE_INPUT_SYSTEM
        private void RegisterForInputDeviceBlocker()
        {
            InputDeviceBlocker.RegisterShouldBlockFuncIfNotYet(InputDeviceBlocker.Device.Pointer, ShouldBlockPointer);
            InputDeviceBlocker.RegisterShouldBlockFuncIfNotYet(InputDeviceBlocker.Device.Mouse, ShouldBlockMouse);
        }
        
        private void UnregisterForInputDeviceBlocker()
        {
            InputDeviceBlocker.UnregisterShouldBlockFunc(InputDeviceBlocker.Device.Pointer, ShouldBlockPointer);
            InputDeviceBlocker.UnregisterShouldBlockFunc(InputDeviceBlocker.Device.Mouse, ShouldBlockMouse);
        }

        private bool ShouldBlockPointer()
        {
            if (!disablePointerInputOverUI) return false;

            var pointer = Pointer.current;
            return pointer is { enabled: true } && IsOverUIInstance(pointer.position.ReadValue());
        }
        
        private bool ShouldBlockMouse()
        {
            if (!disableMouseInputOverUI) return false;

            var mouse = Mouse.current;
            return mouse is { enabled: true } && IsOverUIInstance(mouse.position.ReadValue());
        }
        
        
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