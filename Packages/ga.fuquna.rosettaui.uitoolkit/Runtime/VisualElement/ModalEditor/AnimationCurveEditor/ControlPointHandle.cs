using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class ControlPointHandle : VisualElement
    {
        public delegate void OnTangentChanged(float tangent, float weight);
        
        private VisualElement _lineElement;
        private VisualElement _handleContainerElement;
        private ICoordinateConverter _coordinateConverter;
        
        private readonly OnTangentChanged _onTangentChanged;
        
        private float _angle;
        private float _sign;
        private float _xDistToNeighborInScreen;
        
        private const float DefaultLineLength = 50f;
        private const string HandleRootClassName = "rosettaui-animation-curve-editor__control-point-handle";
        private const string HandleLineClassName = "rosettaui-animation-curve-editor__control-point-handle__line";
        private const string HandleContainerClassName = "rosettaui-animation-curve-editor__control-point-handle__handle-container";
        private const string HandleContainerWeightClassName = "rosettaui-animation-curve-editor__control-point-handle__handle-container-weight";
        private const string HandleClassName = "rosettaui-animation-curve-editor__control-point-handle__handle";
        
        
        public ControlPointHandle(ICoordinateConverter coordinateConverter, float sign, OnTangentChanged onTangentChanged)
        {
            _coordinateConverter = coordinateConverter;
            _sign = sign;
            _onTangentChanged = onTangentChanged;
            AddToClassList(HandleRootClassName);
            InitUI();
            SetTangent(0f);
        }

        private void InitUI()
        {
            _lineElement = new VisualElement();
            _lineElement.AddToClassList(HandleLineClassName);
            Add(_lineElement);
            
            _handleContainerElement = new VisualElement();
            _handleContainerElement.AddToClassList(HandleContainerClassName);
            _lineElement.Add(_handleContainerElement);
            _handleContainerElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            var handleElement = new VisualElement();
            handleElement.AddToClassList(HandleClassName);
            _handleContainerElement.Add(handleElement);
        }
        
        /// <summary>
        /// Set the tangent of the handle
        /// </summary>
        /// <param name="tangent">Tangent in curve coordinate</param>
        public void SetTangent(float tangent)
        {
            float angle = AnimationCurveEditorUtility.GetDegreeFromTangent2(_coordinateConverter.GetScreenTangentFromCurveTangent(tangent) * _sign, _sign);
            if (float.IsNaN(angle)) return;
            
            _lineElement.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
            _angle = angle;
        }
        
        public void SetWeight(float? weight, float xDistToNeighborInScreen)
        {
            _xDistToNeighborInScreen = xDistToNeighborInScreen;
            _lineElement.style.width = weight == null || Mathf.Approximately(_angle, -90f) || Mathf.Approximately(_angle, 90f) || Mathf.Approximately(_angle, 270f)
                ? DefaultLineLength
                : Mathf.Max(10f, (float)weight * Mathf.Abs(xDistToNeighborInScreen / Mathf.Cos(_angle * Mathf.Deg2Rad)));

            if (weight.HasValue)
            {
                _handleContainerElement.AddToClassList(HandleContainerWeightClassName);
            }
            else
            {
                _handleContainerElement.RemoveFromClassList(HandleContainerWeightClassName);
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
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
                tangent = _coordinateConverter.GetCurveTangentFromScreenTangent(tangent);
                if (evt.ctrlKey || evt.commandKey) tangent = Snap(tangent);
                float weight = Mathf.Clamp01(Mathf.Abs(mousePoint.x - centerPoint.x) / Mathf.Abs(_xDistToNeighborInScreen));
                _onTangentChanged?.Invoke(tangent, weight);
            }
            evt.StopPropagation();
            return;

            float Clamp(float xDiff)
            {
                return Mathf.Max(0f, xDiff * _sign) * _sign;
            }

            float Snap(float tangent)
            {
                if (float.IsInfinity(tangent)) return tangent;

                if (Mathf.Abs(tangent) > Mathf.Tan(67.5f))
                {
                    return Mathf.Sign(tangent) * Mathf.Infinity;
                }
                else if (Mathf.Abs(tangent) > Mathf.Tan(22.5f))
                {
                    return Mathf.Sign(tangent);
                }

                return 0f;
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