using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class DragManipulator : PointerManipulator
    {
        public event Action<Vector2> onDragStart;
        public event Action<Vector2> onDrag;
        public event Action onDragEnd;
        
        private bool _active;
        private int _activePointerId = -1;
        private Vector2 _pointerStartPos;   // スクリーン座標(Pointer開始時)
        
        public DragManipulator()
        {
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                modifiers = EventModifiers.None,
            });
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<DetachFromPanelEvent>(_ => CancelDrag());
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        private void OnPointerDown(PointerDownEvent e)
        {
            if (_active) return;

            _active = true;
            _activePointerId = e.pointerId;
            _pointerStartPos = e.position;
            target.CapturePointer(e.pointerId);
            onDragStart?.Invoke(_pointerStartPos);
            e.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (!_active || e.pointerId != _activePointerId) return;
            if (!target.HasPointerCapture(_activePointerId)) return;
            
            var delta = (Vector2)e.position - _pointerStartPos;
            onDrag?.Invoke(delta);
            e.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent e)
        {
            if (!_active || e.pointerId != _activePointerId) return;
            
            Finish();
            e.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent e)
        {
            if (!_active || e.pointerId != _activePointerId) return;
            Finish();
        }

        private void Finish()
        {
            if (!_active) return;
            if (target.HasPointerCapture(_activePointerId))
                target.ReleasePointer(_activePointerId);

            _active = false;
            _activePointerId = -1;
            
            onDragEnd?.Invoke();
        }

        private void CancelDrag()
        {
            if (!_active) return;
            Finish();
        }
    }
}