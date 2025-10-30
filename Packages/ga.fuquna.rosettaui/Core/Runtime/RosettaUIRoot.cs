using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.UndoSystem;
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
        public bool disableKeyboardInputWhileUIFocused = true;
        public bool disablePointerInputOverUI = true;
        public bool disableMouseInputOverUI = true;
        
        public InputAction undoAction;
        public InputAction redoAction;
#else
        [Serializable]
        public class KeyCombination
        {
            [Header("Main Key")]
            public KeyCode key;

            [Header("Modifiers")]
            public bool control;
            public bool command;
            public bool shift;
            public bool alt;
            
            public bool IsPressed()
            {
                if (!Input.GetKeyDown(key)) return false;
                
                if (control != (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) return false;
                if (command != (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))) return false;
                if (shift != (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) return false;
                if (alt != (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) return false;
                
                return true;
            }
        }
        
        public List<KeyCombination> undoKeys = new()
        {
            new KeyCombination {key = KeyCode.Z, control = true},
            new KeyCombination {key = KeyCode.Z, command = true}
        };
        
        public List<KeyCombination> redoKey = new()
        {
            new KeyCombination {key = KeyCode.Y, control = true},
            new KeyCombination {key = KeyCode.Z, command = true, shift = true},
        };
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
            undoAction.Enable();
            redoAction.Enable();
#endif
        }

        protected virtual void OnDisable()
        {
            Unregister(this);
            
#if ENABLE_INPUT_SYSTEM
            UnregisterForInputDeviceBlocker();
            undoAction.Disable();
            redoAction.Disable();
#endif
        }

        
#if ENABLE_INPUT_SYSTEM
        private void Start()
        {
            undoAction.performed += _ => DoUndo();
            redoAction.performed += _ => DoRedo();
        }

#endif

        protected virtual void Update()
        {
#if !ENABLE_INPUT_SYSTEM
            if (IsFocused)
            {
                if (undoKeys.Any(k => k.IsPressed()))
                {
                    DoUndo();
                }
                else if (redoKey.Any(k => k.IsPressed()))
                {
                    DoRedo();
                }
            }
#endif
            
            updater.Update();
        }
        
        // Is there a smarter way to do this?
        private void LateUpdate()
        {
            _undoRedoCalledThisFrame = false;
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

        public abstract bool IsFocusedInstance { get; }
        public abstract bool WillUseKeyInput();
        public abstract bool IsOverUIInstance(Vector2 screenPosition);

        protected abstract void BuildInternal(Element element);


#if ENABLE_INPUT_SYSTEM
        private void RegisterForInputDeviceBlocker()
        {
            InputDeviceBlocker.RegisterShouldBlockFuncIfNotYet(InputDeviceBlocker.Device.Keyboard, ShouldBlockKeyboard);
            InputDeviceBlocker.RegisterShouldBlockFuncIfNotYet(InputDeviceBlocker.Device.Pointer, ShouldBlockPointer);
            InputDeviceBlocker.RegisterShouldBlockFuncIfNotYet(InputDeviceBlocker.Device.Mouse, ShouldBlockMouse);
        }
        
        private void UnregisterForInputDeviceBlocker()
        {
            InputDeviceBlocker.RegisterShouldBlockFuncIfNotYet(InputDeviceBlocker.Device.Keyboard, ShouldBlockKeyboard);
            InputDeviceBlocker.UnregisterShouldBlockFunc(InputDeviceBlocker.Device.Pointer, ShouldBlockPointer);
            InputDeviceBlocker.UnregisterShouldBlockFunc(InputDeviceBlocker.Device.Mouse, ShouldBlockMouse);
        }

        private bool ShouldBlockKeyboard()
        {
            return disableKeyboardInputWhileUIFocused && IsFocusedInstance;
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
#endif

        #region Static

        private static readonly List<RosettaUIRoot> Roots = new();
        
        
        public static bool IsFocused => Roots.Any(root => root.IsFocusedInstance);

        private static void Register(RosettaUIRoot root)
        {
            if(Roots.Contains(root)) return;
            Roots.Add(root);
        }

        private static void Unregister(RosettaUIRoot root)
        {
            Roots.Remove(root);
        }
        
        public static bool IsRootElement(Element element)
        {
            // WindowLauncherElement内のWindowはroot.Elementsに含まれないけどupdater.Elementsには含まれる
            return Roots
                .Where(root => root != null && root.isActiveAndEnabled)
                .Any(root => root.Elements.Contains(element) || root.updater.Elements.Contains(element));
        }

        [Obsolete("Use IsFocused instead")]
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
        
        
        // 複数インスタンスを想定しUndoRedoは１フレームに１回しか呼ばれないようにする
        #region Undo/Redo
        
        private static bool _undoRedoCalledThisFrame;

        private static void DoUndo()
        {
            if (_undoRedoCalledThisFrame) return;
         
            UndoHistory.Undo();
            _undoRedoCalledThisFrame = true;
        }

        private static void DoRedo() 
        {
            if (_undoRedoCalledThisFrame) return;
         
            UndoHistory.Redo();
            _undoRedoCalledThisFrame = true;
        }
        
        #endregion

        #endregion
    }
}