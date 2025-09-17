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
        
        
        public DragManipulator(
            Func<PointerDownEvent, bool> onDragStarting = null,
            Action<PointerMoveEvent> onDrag = null,
            Action<EventBase> onDragEnd = null)
        {
            if (onDrag != null) this.onDrag += (_, evt) => onDrag(evt);
            if (onDragStarting != null) this.onDragStarting += (_, evt) => onDragStarting(evt);
            if (onDragEnd != null) this.onDragEnd += (_, evt) => onDragEnd(evt);
        }
        
        public DragManipulator(
            Func<DragManipulator, PointerDownEvent, bool> onDragStarting ,
            Action<DragManipulator, PointerMoveEvent> onDrag = null,
            Action<DragManipulator, EventBase> onDragEnd = null)
        {
            this.onDragStarting += onDragStarting;
            this.onDrag += onDrag;
            this.onDragEnd += onDragEnd;
        }
        
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
        
        private bool IsCapturedPointer(int pointerId)
        {
            return _activePointerId == pointerId && target.HasPointerCapture(pointerId);
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
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (!IsCapturedPointer(e.pointerId)) return;
            onDrag?.Invoke(this, e);
        }

        private void OnPointerUp(PointerUpEvent e)
        {
            if (!IsCapturedPointer(e.pointerId)) return;
            Finish(e);
        }

        private void OnPointerCancel(PointerCancelEvent e)
        {
            if (!IsCapturedPointer(e.pointerId)) return;
            Finish(e);
        }

        private void CancelDrag(EventBase e)
        {
            if (_activePointerId < 0) return;
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
    }
}