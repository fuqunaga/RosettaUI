using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class ControlPointHandle : VisualElement
    {
        private bool IsLeft => _angleOffset > 90f;

        public delegate void OnTangentChanged(float tangent, float weight);
        
        private VisualElement _lineElement;
        private VisualElement _handleContainerElement;
        private ICoordinateConverter _coordinateConverter;
        
        private Vector2 _mouseDownPosition;
        private Action _onHandleSelected;
        private OnTangentChanged _onTangentChanged;
        private float _angleOffset;
        private float _angle;
        private float _xDistToNeighborInScreen;
        
        private const float DefaultLineLength = 50f;
        
        public ControlPointHandle(ICoordinateConverter coordinateConverter, float angleOffset, Action onHandleSelected, OnTangentChanged onTangentChanged)
        {
            _coordinateConverter = coordinateConverter;
            _angleOffset = angleOffset;
            _onHandleSelected = onHandleSelected;
            _onTangentChanged = onTangentChanged;
            AddToClassList("rosettaui-animation-curve-editor__control-point-handle");
            InitUI();
            SetAngle(0f);
        }

        private void InitUI()
        {
            _lineElement = new VisualElement();
            _lineElement.AddToClassList("rosettaui-animation-curve-editor__control-point-handle__line");
            Add(_lineElement);
            
            _handleContainerElement = new VisualElement();
            _handleContainerElement.AddToClassList("rosettaui-animation-curve-editor__control-point-handle__handle-container");
            _lineElement.Add(_handleContainerElement);
            _handleContainerElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            var handleElement = new VisualElement();
            handleElement.AddToClassList("rosettaui-animation-curve-editor__control-point-handle__handle");
            _handleContainerElement.Add(handleElement);
        }

        /// <summary>
        /// Set the tangent of the handle
        /// </summary>
        /// <param name="tangent">Tangent in curve coordinate</param>
        public void SetTangent(float tangent)
        {
            SetAngle(AnimationCurveEditorUtility.GetDegreeFromTangent(_coordinateConverter.GetScreenTangentFromCurveTangent(tangent)));
        }
        
        public void SetWeight(float? weight, float xDistToNeighborInScreen)
        {
            _xDistToNeighborInScreen = xDistToNeighborInScreen;
            _lineElement.style.width = weight == null || Mathf.Approximately(_angle, -90f) || Mathf.Approximately(_angle, 90f) || Mathf.Approximately(_angle, 270f)
                ? DefaultLineLength
                : Mathf.Max(10f, (float)weight * Mathf.Abs(xDistToNeighborInScreen / Mathf.Cos(_angle * Mathf.Deg2Rad)));
        }
        
        private void SetAngle(float angle)
        {
            if (float.IsNaN(angle)) return;
            angle += _angleOffset;
            _lineElement.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
            _angle = angle;
        }
        
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            _onHandleSelected?.Invoke();
            _handleContainerElement.CaptureMouse();
            _handleContainerElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _handleContainerElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
            evt.StopPropagation();
        }
        
        private void OnPointerMove(PointerMoveEvent evt)
        {
            var centerPoint = parent.worldBound.center;
            var mousePoint = evt.position;
            float tangent = (-mousePoint.y + centerPoint.y) / Clamp(mousePoint.x - centerPoint.x);
            if (!float.IsNaN(tangent))
            {
                tangent =  _coordinateConverter.GetCurveTangentFromScreenTangent(tangent);
                float weight = Mathf.Clamp01(Mathf.Abs(mousePoint.x - centerPoint.x) / Mathf.Abs(_xDistToNeighborInScreen));
                _onTangentChanged?.Invoke(tangent, weight);
            }
            evt.StopPropagation();
            return;

            float Clamp(float xDiff)
            {
                return IsLeft ? Mathf.Min(0f, xDiff) : Mathf.Max(0f, xDiff);
            }
        }
        
        private void OnPointerUp(PointerUpEvent evt)
        {
            _handleContainerElement.ReleaseMouse();
            _handleContainerElement.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            evt.StopPropagation();
        }
    }
}