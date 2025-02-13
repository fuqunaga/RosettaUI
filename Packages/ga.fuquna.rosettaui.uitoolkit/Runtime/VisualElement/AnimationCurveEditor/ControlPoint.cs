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
        
        private bool _isAltPressed = false;
        
        private const string ContainerClassName = "rosettaui-animation-curve-editor__control-point-container";
        private const string ControlPointClassName = "rosettaui-animation-curve-editor__control-point";
        private const string ActiveControlPointClassName = "rosettaui-animation-curve-editor__control-point--active";
        
        public delegate void OnPointAction(ControlPoint controlPoint);
        public delegate int OnPointMoved(Keyframe keyframe);
        
        public ControlPoint(ICoordinateConverter coordinateConverter, 
            OnPointAction onPointSelected, OnPointMoved onPointMoved, OnPointAction onPointRemoved)
        {
            _coordinateConverter = coordinateConverter;
            _onPointSelected = onPointSelected;
            _onPointMoved = onPointMoved;
            _onPointRemoved = onPointRemoved;
            
            focusable = true;
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode is KeyCode.LeftAlt or KeyCode.RightAlt) _isAltPressed = true;
            });
            RegisterCallback<KeyUpEvent>(evt =>
            {
                if (evt.keyCode is KeyCode.LeftAlt or KeyCode.RightAlt) _isAltPressed = false;
            });
            
            // Handles
            _leftHandle = new ControlPointHandle(_coordinateConverter, -1f, 
                () => _onPointSelected(this),
                (tangent, weight) =>
                {
                    _keyframeCopy.inTangent = tangent;
                    _keyframeCopy.inWeight = _keyframeCopy.weightedMode is WeightedMode.In or WeightedMode.Both ? weight : 0.333333f;
                    if (!_isAltPressed)
                    {
                        if (PointMode == PointMode.Flat) PointMode = PointMode.Smooth;
                        if (PointMode == PointMode.Smooth)
                        {
                            if (float.IsInfinity(_keyframeCopy.inTangent)) _keyframeCopy.outTangent = -_keyframeCopy.inTangent;
                            _keyframeCopy.outTangent = _keyframeCopy.inTangent;
                        }
                    }
                    else
                    {
                        SetPointMode(PointMode.Broken);
                    }
                    _onPointMoved(_keyframeCopy);
                }
            );
            Add(_leftHandle);
            
            _rightHandle = new ControlPointHandle(_coordinateConverter, 1f, 
                () => _onPointSelected(this),
                (tangent, weight) =>
                {
                    _keyframeCopy.outTangent = tangent;
                    _keyframeCopy.outWeight = _keyframeCopy.weightedMode is WeightedMode.Out or WeightedMode.Both ? weight : 0.333333f;
                    if (!_isAltPressed)
                    {
                        if (PointMode == PointMode.Flat) PointMode = PointMode.Smooth;
                        if (PointMode == PointMode.Smooth)
                        {
                            if (float.IsInfinity(_keyframeCopy.outTangent)) _keyframeCopy.inTangent = -_keyframeCopy.outTangent;
                            _keyframeCopy.inTangent = _keyframeCopy.outTangent;
                        }
                    }
                    else
                    {
                        SetPointMode(PointMode.Broken);
                    }
                    _onPointMoved(_keyframeCopy);
                }
            );
            Add(_rightHandle);
            
            // Control point container
            AddToClassList(ContainerClassName);
            RegisterCallback<PointerDownEvent>(OnMouseDown);
            
            // Control point
            _controlPoint = new VisualElement { name = "AnimationCurveEditorControlPoint" };
            _controlPoint.AddToClassList(ControlPointClassName);
            Add(_controlPoint);
            
            // Popup menu controller
            _popupMenuController = new ControlPointPopupMenuController(
                () => _onPointRemoved(this),
                SetPointModeAndUpdateView,
                SetTangentModeAndUpdateView,
                ToggleWeightedModeAndUpdateView
            );
            
            return;
            
            void SetPointModeAndUpdateView(PointMode mode)
            {
                SetPointMode(mode);
                switch (mode)
                {
                    case PointMode.Broken:
                        break;
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
            
            void ToggleWeightedModeAndUpdateView(WeightedMode mode)
            {
                _keyframeCopy.ToggleWeightedFrag(mode);
                _onPointMoved(_keyframeCopy);
            }
        }
        
        public void SetActive(bool active)
        {
            if (active)
            {
                _controlPoint.AddToClassList(ActiveControlPointClassName);
                _leftHandle.style.visibility = Visibility.Visible;
                _rightHandle.style.visibility = Visibility.Visible;
            }
            else
            {
                _controlPoint.RemoveFromClassList(ActiveControlPointClassName);
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
            _popupMenuController.SetTangentMode(InTangentMode, OutTangentMode);
        }
        
        public void SetWeightedMode(WeightedMode mode)
        {
            _keyframeCopy.weightedMode = mode;
            _popupMenuController.SetWeightedMode(mode);
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
            _leftHandle.SetWeight(inWeight, GetXDistInScreen(curve, idx - 1, idx));
            _rightHandle.SetWeight(outWeight, GetXDistInScreen(curve, idx, idx + 1));
            return;
            
            float GetXDistInScreen(in AnimationCurve curve, int leftIdx, int rightIdx)
            {
                if (leftIdx < 0 || curve.length <= rightIdx) return 0f;
                return _coordinateConverter.GetScreenPosFromCurvePos(curve[rightIdx].GetPosition()).x - _coordinateConverter.GetScreenPosFromCurvePos(curve[leftIdx].GetPosition()).x;
            }
        }
        
        private void SetPointerPosition(Vector2 pointerPosition)
        {
            Vector2 screenPosition = new Vector2(_elementPositionOnDown.x + (pointerPosition.x - _mouseDownPosition.x), _elementPositionOnDown.y + (pointerPosition.y - _mouseDownPosition.y));
            style.left = screenPosition.x;
            style.top = screenPosition.y;
            PositionInCurve = _coordinateConverter.GetCurvePosFromScreenPos(screenPosition);
            _onPointMoved(_keyframeCopy);
        }
        
        private void OnMouseDown(PointerDownEvent evt)
        {
            _mouseDownPosition = evt.position;
            _elementPositionOnDown = new Vector2(style.left.value.value, style.top.value.value);
            _onPointSelected?.Invoke(this);
            if (evt.button == 0)
            {
                RegisterCallback<PointerMoveEvent>(OnPointerMove);
                RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
                RegisterCallback<PointerUpEvent>(OnPointerUp);
                evt.StopPropagation();
                this.CaptureMouse();
            }
            else if (evt.button == 1)
            {
                _popupMenuController.Show(_mouseDownPosition, this);
                evt.StopPropagation();
            }
        }
        
        private void OnPointerMove(PointerMoveEvent evt)
        {
            SetPointerPosition(evt.position);
        }
        
        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            SetPointerPosition(evt.position);
        }
        
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button == 0)
            {
                UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
                UnregisterCallback<PointerUpEvent>(OnPointerUp);
                evt.StopPropagation();
                this.ReleaseMouse();
            }
        }
        
    }
}