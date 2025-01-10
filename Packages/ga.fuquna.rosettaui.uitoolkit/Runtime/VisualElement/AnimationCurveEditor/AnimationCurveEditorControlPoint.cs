using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Control point of the animation curve editor.
    /// </summary>
    public class AnimationCurveEditorControlPoint : VisualElement
    {
        private Action<Vector2> _onPointMoved;
        private Vector2 _elementPositionOnDown;
        private Vector2 _mouseDownPosition;
        
        private const float ControlPointSize = 10f;
        
        public AnimationCurveEditorControlPoint(Action<Vector2> onPointMoved)
        {
            _onPointMoved = onPointMoved;
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            
            // Styles
            AddToClassList("rosettaui-animation-curve-editor__control-point");
            style.width = ControlPointSize;
            style.height = ControlPointSize;
        }
        
        public void SetPosition(float x, float y)
        {
            style.left = x - ControlPointSize * 0.5f;
            style.top = y - ControlPointSize * 0.5f;
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            _mouseDownPosition = evt.mousePosition;
            _elementPositionOnDown = new Vector2(style.left.value.value, style.top.value.value);
            evt.StopPropagation();
            this.CaptureMouse();
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            style.left = _elementPositionOnDown.x + (evt.mousePosition.x - _mouseDownPosition.x);
            style.top = _elementPositionOnDown.y + (evt.mousePosition.y - _mouseDownPosition.y);
            _onPointMoved?.Invoke(Vector2.up + new Vector2(style.left.value.value, -style.top.value.value) / parent.layout.size);
        }
        
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            style.left = _elementPositionOnDown.x + (evt.mousePosition.x - _mouseDownPosition.x);
            style.top = _elementPositionOnDown.y + (evt.mousePosition.y - _mouseDownPosition.y);
            _onPointMoved?.Invoke(Vector2.up + new Vector2(style.left.value.value, -style.top.value.value) / parent.layout.size);
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {
                UnregisterCallback<MouseMoveEvent>(OnMouseMove);
                UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
                UnregisterCallback<MouseUpEvent>(OnMouseUp);
                evt.StopPropagation();
                this.ReleaseMouse();
            }
        }
        
        
        
    }
}