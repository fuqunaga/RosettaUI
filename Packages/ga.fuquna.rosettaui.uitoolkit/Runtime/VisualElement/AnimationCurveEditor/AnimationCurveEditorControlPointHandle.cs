using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class AnimationCurveEditorControlPointHandle : VisualElement
    {
        private VisualElement _lineElement;
        private VisualElement _handleElement;
        
        private Vector2 _mouseDownPosition;
        
        public AnimationCurveEditorControlPointHandle(float angle)
        {
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
            _handleElement.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }
        
        public void SetAngle(float angle)
        {
            _lineElement.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(180f - angle)));
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;
            _mouseDownPosition = evt.mousePosition;
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
            var angle = Mathf.Atan2(mousePoint.y - centerPoint.y, mousePoint.x - centerPoint.x) * Mathf.Rad2Deg;
            SetAngle(180f - angle);
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