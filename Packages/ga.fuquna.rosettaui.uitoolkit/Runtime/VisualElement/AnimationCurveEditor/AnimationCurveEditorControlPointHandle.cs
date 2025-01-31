using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class AnimationCurveEditorControlPointHandle : VisualElement
    {
        private VisualElement _lineElement;
        private VisualElement _handleElement;
        
        private Vector2 _mouseDownPosition;
        private Action _onHandleSelected;
        private Action<float> _onAngleChanged;
        
        public AnimationCurveEditorControlPointHandle(float angle, Action onHandleSelected, Action<float> onAngleChanged)
        {
            _onHandleSelected = onHandleSelected;
            _onAngleChanged = onAngleChanged;
            AddToClassList("rosettaui-animation-curve-editor__control-point-handle");
            InitUI();
            SetAngle(angle);
        }

        private void InitUI()
        {
            _lineElement = new VisualElement();
            _lineElement.AddToClassList("rosettaui-animation-curve-editor__control-point-handle__line");
            Add(_lineElement);
            
            _handleElement = new VisualElement();
            _handleElement.AddToClassList("rosettaui-animation-curve-editor__control-point-handle__handle");
            _lineElement.Add(_handleElement);
            _handleElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
        }
        
        public void SetAngle(float angle)
        {
            if (Mathf.Approximately(Mathf.Abs(angle), 90f)) angle *= -1f;
            _lineElement.transform.rotation = Quaternion.AngleAxis(angle + 180f, Vector3.back);
        }
        
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            _onHandleSelected?.Invoke();
            _handleElement.CaptureMouse();
            _handleElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _handleElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            _handleElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
            evt.StopPropagation();
        }
        
        private void OnPointerMove(PointerMoveEvent evt)
        {
            var centerPoint = parent.LocalToWorld(Vector2.one * 0.5f);
            var mousePoint = evt.position;
            var angle = Mathf.Atan2(-mousePoint.y + centerPoint.y, mousePoint.x - centerPoint.x) * Mathf.Rad2Deg;

            Debug.Log(angle);
            SetAngle(angle);
            _onAngleChanged?.Invoke(angle);
            evt.StopPropagation();
        }
        
        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            // _handleElement.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            // _handleElement.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
        
        private void OnPointerUp(PointerUpEvent evt)
        {
            _handleElement.ReleaseMouse();
            _handleElement.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _handleElement.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }
    }
}