using System;
using System.Diagnostics.CodeAnalysis;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Control point of the animation curve editor.
    /// </summary>
    public class ControlPoint : VisualElement
    {
        public event Action<ControlPoint> onKeyframeChanged;
        
        private readonly OnPointAction _onPointSelected;
        private readonly Action<ControlPoint, Vector2> _onPointMoved;
        
        private readonly ICoordinateConverter _coordinateConverter;
        private readonly ParameterPopup _parameterPopup;
        private readonly EditKeyPopup _editKeyPopup;
        private readonly VisualElement _controlPoint;
        private readonly ControlPointHandle _leftHandle;
        private readonly ControlPointHandle _rightHandle;
        private readonly ControlPointPopupMenuController _popupMenuController;


        private Vector2 _mouseDownPositionToElementOffset;
        private Keyframe _keyframeCopy;
        
        private readonly VisualElementKeyEventHelper _keyEventHelper;
        
        private const string ContainerClassName = "rosettaui-animation-curve-editor__control-point-container";
        private const string ControlPointClassName = "rosettaui-animation-curve-editor__control-point";
        private const string ActiveControlPointClassName = "rosettaui-animation-curve-editor__control-point--active";
        
        public delegate void OnPointAction(ControlPoint controlPoint);
        
        
        public PointMode PointMode { get; private set; } = PointMode.Smooth;
        public TangentMode InTangentMode { get; private set; } = TangentMode.Free;
        public TangentMode OutTangentMode { get; private set; } = TangentMode.Free;

        public bool IsActive
        {
            get => _controlPoint.ClassListContains(ActiveControlPointClassName);
            set{
                if (value)
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
        }
        
        public Keyframe Keyframe => _keyframeCopy;
        
        public Vector2 KeyframePosition
        {
            get => new(_keyframeCopy.time, _keyframeCopy.value);
            
            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
            set
            {
                if ( _keyframeCopy.time == value.x && _keyframeCopy.value == value.y) return;
                
                _keyframeCopy.time = value.x;
                _keyframeCopy.value = value.y;
                
                onKeyframeChanged?.Invoke(this);
            }
        }
        
        public ControlPoint(ICoordinateConverter coordinateConverter, ParameterPopup parameterPopup, EditKeyPopup editKeyPopup,
            OnPointAction onPointSelected, Action<ControlPoint, Vector2> onPointMoved)
        {
            _coordinateConverter = coordinateConverter;
            _parameterPopup = parameterPopup;
            _editKeyPopup = editKeyPopup;
            _onPointSelected = onPointSelected;
            _onPointMoved = onPointMoved;
            
            _keyEventHelper = new VisualElementKeyEventHelper(this);
            _keyEventHelper.RegisterKeyAction(new[] { KeyCode.LeftAlt, KeyCode.RightAlt }, type =>
            {
                if (type != KeyEventType.KeyDown) return;
                SetPointMode(PointMode.Broken);
            });
            
            // Handles
            _leftHandle = new ControlPointHandle(_coordinateConverter, -1f, 
                () => _onPointSelected(this),
                (tangent, weight) =>
                {
                    _keyframeCopy.inTangent = tangent;
                    _keyframeCopy.inWeight = _keyframeCopy.weightedMode is WeightedMode.In or WeightedMode.Both ? weight : 0.333333f;
                    if (PointMode == PointMode.Flat) { PointMode = PointMode.Smooth; }
                    if (PointMode == PointMode.Smooth)
                    {
                        if (float.IsInfinity(_keyframeCopy.inTangent)) _keyframeCopy.outTangent = -_keyframeCopy.inTangent;
                        _keyframeCopy.outTangent = _keyframeCopy.inTangent;
                    }
                    // _onPointMoved(_keyframeCopy);
                }
            );
            Add(_leftHandle);
            
            _rightHandle = new ControlPointHandle(_coordinateConverter, 1f, 
                () => _onPointSelected(this),
                (tangent, weight) =>
                {
                    _keyframeCopy.outTangent = tangent;
                    _keyframeCopy.outWeight = _keyframeCopy.weightedMode is WeightedMode.Out or WeightedMode.Both ? weight : 0.333333f;
                    if (PointMode == PointMode.Flat) { PointMode = PointMode.Smooth; }
                    if (PointMode == PointMode.Smooth)
                    {
                        if (float.IsInfinity(_keyframeCopy.outTangent)) _keyframeCopy.inTangent = -_keyframeCopy.outTangent;
                        _keyframeCopy.inTangent = _keyframeCopy.outTangent;
                    }
                    // _onPointMoved(_keyframeCopy);
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
                // () => _onPointRemoved(this),
                () => throw new NotImplementedException("onPointRemoved is not implemented in ControlPoint"),
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
                // _onPointMoved(_keyframeCopy);
            }
            
            void SetTangentModeAndUpdateView(TangentMode? inTangentMode, TangentMode? outTangentMode)
            {
                SetTangentMode(inTangentMode, outTangentMode);
                // _onPointMoved(_keyframeCopy);
            }
            
            void ToggleWeightedModeAndUpdateView(WeightedMode mode)
            {
                _keyframeCopy.ToggleWeightedFrag(mode);
                // _onPointMoved(_keyframeCopy);
                _popupMenuController.SetWeightedMode(_keyframeCopy.weightedMode);
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
            UpdateView();
            
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


        public void UpdateView()
        {
            var uiPosition = _coordinateConverter.GetScreenPosFromCurvePos(KeyframePosition);
            style.left = uiPosition.x;
            style.top = uiPosition.y;
        }
        
        private void OnMouseDown(PointerDownEvent evt)
        {
            var elementPosition = new Vector2(resolvedStyle.left, resolvedStyle.top);
            _mouseDownPositionToElementOffset = elementPosition - (Vector2)evt.position; 
            _onPointSelected?.Invoke(this);
            
            switch (evt.button)
            {
                case 0:
                    RegisterCallback<PointerMoveEvent>(OnPointerMove);
                    RegisterCallback<PointerUpEvent>(OnPointerUp);
                    evt.StopPropagation();
                    this.CaptureMouse();
                    
                    _parameterPopup.Show();
                    _parameterPopup.Update(elementPosition, _keyframeCopy);
                    break;
                case 1:
                    _popupMenuController.Show(evt.position, this);
                    evt.StopPropagation();
                    break;
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            var screenPosition = (Vector2)evt.position + _mouseDownPositionToElementOffset;
            
            // Snap to gridモードがあるのでポインターの移動量がそのまま反映されないことがある
            // _onPointMovedに値の更新は任せる
            var desireKeyframePosition = _coordinateConverter.GetCurvePosFromScreenPos(screenPosition);
            _onPointMoved(this, desireKeyframePosition);

            _parameterPopup.Update(screenPosition, Keyframe);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button == 0)
            {
                UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                UnregisterCallback<PointerUpEvent>(OnPointerUp);
                evt.StopPropagation();
                this.ReleaseMouse();
                
                _parameterPopup.Hide();
            }
        }

        public void ShowEditKeyPopup()
        {
            _editKeyPopup.Show(this);
        }
    }
}