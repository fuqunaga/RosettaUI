using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public enum KeyEventType
    {
        KeyDown,
        KeyPress,
        KeyUp
    }
    
    public class VisualElementKeyEventHelper
    {
        private readonly VisualElement _target;
        private readonly Dictionary<KeyCode, List<Action<KeyEventType>>> _keyActions = new();
        private readonly HashSet<KeyCode> _pressedKeys = new();
        
        public VisualElementKeyEventHelper(VisualElement target)
        {
            _target = target;
            _target.focusable = true;
            _target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            _target.RegisterCallback<KeyUpEvent>(OnKeyUp);
        }
        
        ~VisualElementKeyEventHelper()
        {
            _target?.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            _target?.UnregisterCallback<KeyUpEvent>(OnKeyUp);
            _keyActions.Clear();
            _pressedKeys.Clear();
        }
        
        public IDisposable RegisterKeyAction(KeyCode[] keyCodes, Action<KeyEventType> action)
        {
            foreach (var code in keyCodes)
            {
                RegisterKeyAction(code, action);
            }
            return new Disposer(this, keyCodes, action);
        }
        
        public IDisposable RegisterKeyAction(KeyCode keyCode, Action<KeyEventType> action)
        {
            if (!_keyActions.TryGetValue(keyCode, out var actions))
            {
                actions = new List<Action<KeyEventType>>();
                _keyActions[keyCode] = actions;
            }
            actions.Add(action);
            return new Disposer(this, new[] { keyCode }, action);
        }
        
        public bool IsKeyPressed(KeyCode keyCode)
        {
            return _pressedKeys.Contains(keyCode);
        }
        
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!_keyActions.TryGetValue(evt.keyCode, out var actions)) return;
            foreach (var action in actions)
            {
                action(_pressedKeys.Contains(evt.keyCode) ? KeyEventType.KeyPress : KeyEventType.KeyDown);
            }
            _pressedKeys.Add(evt.keyCode);
        }
        
        private void OnKeyUp(KeyUpEvent evt)
        {
            _pressedKeys.Remove(evt.keyCode);
            if (!_keyActions.TryGetValue(evt.keyCode, out var actions)) return;
            foreach (var action in actions)
            {
                action(KeyEventType.KeyUp);
            }
        }
        
        private class Disposer : IDisposable
        {
            private readonly VisualElementKeyEventHelper _helper;
            private readonly KeyCode[] _targetKeyCodes;
            private readonly Action<KeyEventType> _targetAction;
            
            public Disposer(VisualElementKeyEventHelper helper, KeyCode[] keyCodes, Action<KeyEventType> action)
            {
                _helper = helper;
                _targetKeyCodes = keyCodes;
                _targetAction = action;
            }
            
            public void Dispose()
            {
                foreach (var keyCode in _targetKeyCodes)
                {
                    if (!_helper._keyActions.TryGetValue(keyCode, out var actions)) continue;
                    actions.Remove(_targetAction); 
                }
            }
        }

    }
}