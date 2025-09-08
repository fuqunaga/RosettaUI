using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class DragManipulator : PointerManipulator
    {
        public event Func<DragManipulator, PointerDownEvent, bool> onDragStarting;
        public event Action<DragManipulator, PointerMoveEvent> onDrag;
        public event Action<DragManipulator, EventBase> onDragEnd;
        
        private int _activePointerId = -1;
        
        // public DragManipulator()
        // {
        //     activators.Add(new ManipulatorActivationFilter
        //     {
        //         button = MouseButton.LeftMouse,
        //         modifiers = EventModifiers.None,
        //     });
        // }
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<DetachFromPanelEvent>(CancelDrag);
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
            if (_activePointerId >= 0) return;

            var startDrag = onDragStarting?.Invoke(this, e) ?? true;
            if (startDrag)
            {
                _activePointerId = e.pointerId;
                target.CapturePointer(e.pointerId);
            }

            e.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (e.pointerId != _activePointerId) return;
            if (!target.HasPointerCapture(_activePointerId)) return;
            
            onDrag?.Invoke(this, e);
            e.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent e)
        {
            if (e.pointerId != _activePointerId) return;
            
            Finish(e);
            e.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent e)
        {
            if (e.pointerId != _activePointerId) return;
            Finish(e);
        }

        private void Finish(EventBase e)
        {
            if (_activePointerId >= 0 && target.HasPointerCapture(_activePointerId))
            {
                target.ReleasePointer(_activePointerId);
            }

            _activePointerId = -1;
            
            onDragEnd?.Invoke(this, e);
        }

        private void CancelDrag(EventBase e)
        {
            Finish(e);
        }
    }
}