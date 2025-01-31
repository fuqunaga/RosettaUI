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
        private Action<AnimationCurveEditorControlPoint> _onPointSelected;
        private OnPointMoved _onPointMoved;
        private Vector2 _elementPositionOnDown;
        private Vector2 _mouseDownPosition;
        
        private VisualElement _controlPoint;
        private AnimationCurveEditorControlPointHandle _leftHandle;
        private AnimationCurveEditorControlPointHandle _rightHandle;

        private float _inTangent;
        private float _outTangent;

        private const float ControlPointSize = 10f;
        
        public delegate int OnPointMoved(Vector2 position, float inTan, float outTan);
        
        public AnimationCurveEditorControlPoint(Action<AnimationCurveEditorControlPoint> onPointSelected, OnPointMoved onPointMoved)
        {
            _onPointSelected = onPointSelected;
            _onPointMoved = onPointMoved;
            
            // Handles
            _leftHandle = new AnimationCurveEditorControlPointHandle(0f);
            Add(_leftHandle);
            _rightHandle = new AnimationCurveEditorControlPointHandle(180f);
            Add(_rightHandle);
            
            // Control point
            _controlPoint = new VisualElement();
            _controlPoint.RegisterCallback<MouseDownEvent>(OnMouseDown);
            Add(_controlPoint);
            
            // Styles
            _controlPoint.AddToClassList("rosettaui-animation-curve-editor__control-point");
            _controlPoint.style.width = ControlPointSize;
            _controlPoint.style.height = ControlPointSize;
        }
        
        public void SetPosition(float x, float y, float leftTan, float rightTan)
        {
            style.left = x - ControlPointSize * 0.5f;
            style.top = y - ControlPointSize * 0.5f;
            _leftHandle.SetAngle(Mathf.Atan(leftTan) * Mathf.Rad2Deg);
            _rightHandle.SetAngle(180f + Mathf.Atan(rightTan) * Mathf.Rad2Deg);
            _inTangent = leftTan;
            _outTangent = rightTan;
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            _mouseDownPosition = evt.mousePosition;
            _elementPositionOnDown = new Vector2(style.left.value.value, style.top.value.value);
            _onPointSelected?.Invoke(this);
            evt.StopPropagation();
            this.CaptureMouse();
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            SetMousePosition(evt.mousePosition);
        }
        
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            SetMousePosition(evt.mousePosition);
        }
        
        private void SetMousePosition(Vector2 mousePosition)
        {
            style.left = _elementPositionOnDown.x + (mousePosition.x - _mouseDownPosition.x);
            style.top = _elementPositionOnDown.y + (mousePosition.y - _mouseDownPosition.y);
            _onPointMoved.Invoke(
                Vector2.up + new Vector2(style.left.value.value, 
                    -style.top.value.value) / parent.layout.size,
                _inTangent,
                _outTangent
            );
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