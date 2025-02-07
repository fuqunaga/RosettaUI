using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Control point of the animation curve editor.
    /// </summary>
    public class ControlPoint : VisualElement
    {
        public PointMode PointMode { get; private set; } = PointMode.Smooth;
        public TangentMode InTangentMode { get; private set; } = TangentMode.Free;
        public TangentMode OutTangentMode { get; private set; } = TangentMode.Free;
        
        private Vector2 PositionInCurve
        {
            get => new Vector2(_keyframeCopy.time, _keyframeCopy.value);
            set
            {
                _keyframeCopy.time = value.x;
                _keyframeCopy.value = value.y;
            }
        }

        private OnPointAction _onPointSelected;
        private OnPointMoved _onPointMoved;
        private OnPointAction _onPointRemoved;
        
        private Vector2 _elementPositionOnDown;
        private Vector2 _mouseDownPosition;
        
        private ICoordinateConverter _coordinateConverter;
        private VisualElement _controlPoint;
        private ControlPointHandle _leftHandle;
        private ControlPointHandle _rightHandle;
        private ControlPointPopupMenuController _popupMenuController;

        private Keyframe _keyframeCopy;
        
        
        public delegate void OnPointAction(ControlPoint controlPoint);
        public delegate int OnPointMoved(Keyframe keyframe);
        
        public ControlPoint(ICoordinateConverter coordinateConverter, 
            OnPointAction onPointSelected, OnPointMoved onPointMoved, OnPointAction onPointRemoved)
        {
            _coordinateConverter = coordinateConverter;
            _onPointSelected = onPointSelected;
            _onPointMoved = onPointMoved;
            _onPointRemoved = onPointRemoved;
            
            // Handles
            _leftHandle = new ControlPointHandle(_coordinateConverter, 180f, 
                () => _onPointSelected(this),
                (tangent, weight) =>
                {
                    _keyframeCopy.inTangent = tangent;
                    _keyframeCopy.inWeight = InTangentMode == TangentMode.Weighted ? weight : 0.333333f;
                    if (!InputUtility.GetKey(KeyCode.LeftAlt) && !InputUtility.GetKey(KeyCode.RightAlt))
                    {
                        if (PointMode == PointMode.Flat) PointMode = PointMode.Smooth;
                        if (PointMode == PointMode.Smooth)
                        {
                            if (float.IsInfinity(_keyframeCopy.inTangent)) _keyframeCopy.outTangent = -_keyframeCopy.inTangent;
                            _keyframeCopy.outTangent = _keyframeCopy.inTangent;
                        }
                    }
                    _onPointMoved(_keyframeCopy);
                }
            );
            Add(_leftHandle);
            
            _rightHandle = new ControlPointHandle(_coordinateConverter, 0f, 
                () => _onPointSelected(this),
                (tangent, weight) =>
                {
                    _keyframeCopy.outTangent = tangent;
                    _keyframeCopy.outWeight = OutTangentMode == TangentMode.Weighted ? weight : 0.333333f;
                    if (!InputUtility.GetKey(KeyCode.LeftAlt) && !InputUtility.GetKey(KeyCode.RightAlt))
                    {
                        if (PointMode == PointMode.Flat) PointMode = PointMode.Smooth;
                        if (PointMode == PointMode.Smooth)
                        {
                            if (float.IsInfinity(_keyframeCopy.outTangent)) _keyframeCopy.inTangent = -_keyframeCopy.outTangent;
                            _keyframeCopy.inTangent = _keyframeCopy.outTangent;
                        }
                    }
                    _onPointMoved(_keyframeCopy);
                }
            );
            Add(_rightHandle);
            
            // Control point container
            AddToClassList("rosettaui-animation-curve-editor__control-point-container");
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            
            // Control point
            _controlPoint = new VisualElement { name = "AnimationCurveEditorControlPoint" };
            _controlPoint.AddToClassList("rosettaui-animation-curve-editor__control-point");
            Add(_controlPoint);
            
            // Popup menu controller
            _popupMenuController = new ControlPointPopupMenuController(
                () => _onPointRemoved(this),
                SetPointModeAndUpdateView,
                SetTangentModeAndUpdateView
            );
            
            return;
            
            void SetPointModeAndUpdateView(PointMode mode)
            {
                SetPointMode(mode);
                switch (mode)
                {
                    case PointMode.Broken:
                        return;
                    case PointMode.Flat:
                        _keyframeCopy.inTangent = 0f;
                        _keyframeCopy.outTangent = 0f;
                        break;
                    case PointMode.Smooth:
                        _keyframeCopy.inTangent = _keyframeCopy.outTangent;
                        break;
                }
                _onPointMoved(_keyframeCopy);
            }
            
            void SetTangentModeAndUpdateView(TangentMode? inTangentMode, TangentMode? outTangentMode)
            {
                SetTangentMode(inTangentMode, outTangentMode);
                _onPointMoved(_keyframeCopy);
            }
        }
        
        public void SetActive(bool active)
        {
            if (active)
            {
                _controlPoint.style.backgroundColor = new StyleColor(Color.white);
                _leftHandle.style.visibility = Visibility.Visible;
                _rightHandle.style.visibility = Visibility.Visible;
            }
            else
            {
                _controlPoint.style.backgroundColor = new StyleColor(Color.green);
                _leftHandle.style.visibility = Visibility.Hidden;
                _rightHandle.style.visibility = Visibility.Hidden;
            }
        }
        
        public void SetPointMode(PointMode mode)
        {
            PointMode = mode;
            _popupMenuController.SetPointMode(mode);

            if (mode != PointMode.Broken)
            {
                SetTangentMode(TangentMode.Free, TangentMode.Free);
            }
        }
        
        public void SetTangentMode(TangentMode? inTangentMode, TangentMode? outTangentMode)
        {
            InTangentMode = inTangentMode ?? InTangentMode;
            OutTangentMode = outTangentMode ?? OutTangentMode;
            _keyframeCopy.weightedMode = this.GetWeightedMode();
            _popupMenuController.SetTangentMode(InTangentMode, OutTangentMode);
        }
        
        /// <summary>
        /// Set the keyframe of the control point
        /// (without applying the point mode or tangent mode)
        /// </summary>
        public void SetKeyframe(in AnimationCurve curve, int idx)
        {
            _keyframeCopy = curve[idx];
            
            // Set Position
            var screenPosition = _coordinateConverter.GetScreenPosFromCurvePos(PositionInCurve);
            style.left = screenPosition.x;
            style.top = screenPosition.y;
            _leftHandle.SetTangent(_keyframeCopy.inTangent);
            _rightHandle.SetTangent(_keyframeCopy.outTangent);
            
            float? inWeight = _keyframeCopy.weightedMode is WeightedMode.In or WeightedMode.Both ? _keyframeCopy.inWeight : null;
            float? outWeight = _keyframeCopy.weightedMode is WeightedMode.Out or WeightedMode.Both ? _keyframeCopy.outWeight : null;
            _leftHandle.SetWeight(inWeight, GetXDistInScreen(curve, idx, idx - 1));
            _rightHandle.SetWeight(outWeight, GetXDistInScreen(curve, idx + 1, idx));
            return;
            
            float GetXDistInScreen(in AnimationCurve curve, int leftIdx, int rightIdx)
            {
                if (rightIdx <= 0 || curve.length <= leftIdx) return 0f;
                return _coordinateConverter.GetScreenPosFromCurvePos(curve[leftIdx].GetPosition()).x - _coordinateConverter.GetScreenPosFromCurvePos(curve[rightIdx].GetPosition()).x;
            }
        }
        
        private void SetMousePosition(Vector2 mousePosition)
        {
            Vector2 screenPosition = new Vector2(_elementPositionOnDown.x + (mousePosition.x - _mouseDownPosition.x), _elementPositionOnDown.y + (mousePosition.y - _mouseDownPosition.y));
            style.left = screenPosition.x;
            style.top = screenPosition.y;
            PositionInCurve = _coordinateConverter.GetCurvePosFromScreenPos(screenPosition);
            _onPointMoved(_keyframeCopy);
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            _mouseDownPosition = evt.mousePosition;
            _elementPositionOnDown = new Vector2(style.left.value.value, style.top.value.value);
            _onPointSelected?.Invoke(this);
            if (evt.button == 0)
            {
                RegisterCallback<MouseMoveEvent>(OnMouseMove);
                RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
                RegisterCallback<MouseUpEvent>(OnMouseUp);
                evt.StopPropagation();
                this.CaptureMouse();
            }
            else if (evt.button == 1)
            {
                _popupMenuController.Show(_mouseDownPosition, this);
            }
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            SetMousePosition(evt.mousePosition);
        }
        
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            SetMousePosition(evt.mousePosition);
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