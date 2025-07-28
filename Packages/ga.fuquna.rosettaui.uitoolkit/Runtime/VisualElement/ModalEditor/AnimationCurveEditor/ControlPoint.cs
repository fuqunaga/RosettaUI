using System;
using System.Diagnostics.CodeAnalysis;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Control point of the animation curve editor.
    /// 
    /// ControlPointはAnimationCurveのKeyframeと1対1対応するが、直接Keyframeを編集はしない
    /// Keyframeの値を変えると隣り合ったKeyframeの値も変更する必要があることがある（インデックスが変わったり、TangentモードがLinearだとTangentが変わるなど）ので、
    /// ControlPoint単体でKeyframeを編集せずにAnimationCurve全体での整合性を取るようControlPointHolderなどに変更を依頼する
    /// </summary>
    public class ControlPoint : VisualElement
    {
        private const string ContainerClassName = "rosettaui-animation-curve-editor__control-point-container";
        private const string ControlPointClassName = "rosettaui-animation-curve-editor__control-point";
        private const string ActiveControlPointClassName = "rosettaui-animation-curve-editor__control-point--active";

        private readonly OnPointAction _onPointSelected;
        private readonly Action<ControlPoint, Vector2> _onPointMoved;

        private readonly ControlPointHolder _holder;
        private readonly ICoordinateConverter _coordinateConverter;
        private readonly ParameterPopup _parameterPopup;
        private readonly EditKeyPopup _editKeyPopup;
        private readonly VisualElement _controlPoint;
        private readonly ControlPointHandle _leftHandle;
        private readonly ControlPointHandle _rightHandle;
        private readonly ControlPointPopupMenuController _popupMenuController;


        private Vector2 _pointerDownPositionToElementOffset;


        public delegate void OnPointAction(ControlPoint controlPoint);
        
        
        public PointMode PointMode { get; private set; } = PointMode.Smooth;
        public TangentMode InTangentMode { get; private set; } = TangentMode.Free;
        public TangentMode OutTangentMode { get; private set; } = TangentMode.Free;

        public bool IsActive
        {
            get => _controlPoint.ClassListContains(ActiveControlPointClassName);
            set
            {
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
        
        public Keyframe Keyframe => _holder.GetKeyframe(this);
        
        public Vector2 KeyframePosition
        {
            get
            {
                var keyframe = Keyframe;
                return new Vector2(keyframe.time, keyframe.value);
            }
            set => _holder.UpdateKeyframePosition(this, value);
        }
        
        private ControlPoint Left => _holder.GetControlPointLeft(this);
        private ControlPoint Right => _holder.GetControlPointRight(this);

        public ControlPoint(ControlPointHolder holder, ICoordinateConverter coordinateConverter, ParameterPopup parameterPopup, EditKeyPopup editKeyPopup,
            OnPointAction onPointSelected, Action<ControlPoint, Vector2> onPointMoved)
        {
            _holder = holder;
            _coordinateConverter = coordinateConverter;
            _parameterPopup = parameterPopup;
            _editKeyPopup = editKeyPopup;
            _onPointSelected = onPointSelected;
            _onPointMoved = onPointMoved;
            
            var keyEventHelper = new VisualElementKeyEventHelper(this);
            keyEventHelper.RegisterKeyAction(new[] { KeyCode.LeftAlt, KeyCode.RightAlt }, type =>
            {
                if (type != KeyEventType.KeyDown) return;
                SetPointMode(PointMode.Broken);
            });
            
            // Handles
            _leftHandle = new ControlPointHandle(_coordinateConverter, -1f, 
                (tangent, weight) =>
                {
                    var keyframe = Keyframe;
                    keyframe.inTangent = tangent;
                    keyframe.inWeight = keyframe.weightedMode is WeightedMode.In or WeightedMode.Both ? weight : 0.333333f;
                    if (PointMode == PointMode.Flat) { PointMode = PointMode.Smooth; }
                    if (PointMode == PointMode.Smooth)
                    {
                        if (float.IsInfinity(keyframe.inTangent)) keyframe.outTangent = -keyframe.inTangent;
                        keyframe.outTangent = keyframe.inTangent;
                    }
                    
                    _holder.UpdateKeyframe(this, keyframe);
                }
            );
            Add(_leftHandle);
            
            _rightHandle = new ControlPointHandle(_coordinateConverter, 1f, 
                (tangent, weight) =>
                {
                    var keyframe = Keyframe;
                    keyframe.outTangent = tangent;
                    keyframe.outWeight = keyframe.weightedMode is WeightedMode.Out or WeightedMode.Both ? weight : 0.333333f;
                    if (PointMode == PointMode.Flat) { PointMode = PointMode.Smooth; }
                    if (PointMode == PointMode.Smooth)
                    {
                        if (float.IsInfinity(keyframe.outTangent)) keyframe.inTangent = -keyframe.outTangent;
                        keyframe.inTangent = keyframe.outTangent;
                    }
                    
                    _holder.UpdateKeyframe(this, keyframe);
                }
            );
            Add(_rightHandle);
            
            // Control point container
            AddToClassList(ContainerClassName);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            
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
                        // TODO
                        // _keyframeCopy.inTangent = 0f;
                        // _keyframeCopy.outTangent = 0f;
                        break;
                    case PointMode.Smooth:
                        // TODO
                        // _keyframeCopy.inTangent = _keyframeCopy.outTangent;
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
                // TODO
                // _keyframeCopy.ToggleWeightedFrag(mode);
                // _onPointMoved(_keyframeCopy);
                // _popupMenuController.SetWeightedMode(_keyframeCopy.weightedMode);
            }
        }
        
        public void SetPointMode(PointMode mode)
        {
            PointMode = mode;

            if (mode != PointMode.Broken)
            {
                SetTangentMode(TangentMode.Free, TangentMode.Free);
            }
        }
        
        public void SetTangentMode(TangentMode? inTangentMode, TangentMode? outTangentMode)
        {
            InTangentMode = inTangentMode ?? InTangentMode;
            OutTangentMode = outTangentMode ?? OutTangentMode;

            if (InTangentMode == TangentMode.Constant || OutTangentMode == TangentMode.Constant)
            {
                PointMode = PointMode.Broken;
            }
        }
        
        public void UpdateView()
        {
            var keyframe = Keyframe;
            
            var uiPosition = _coordinateConverter.GetScreenPosFromCurvePos(keyframe.GetPosition());
            style.left = uiPosition.x;
            style.top = uiPosition.y;

            if (IsActive)
            {
                UpdateHandleView();
            }
        }

        private void UpdateHandleView()
        {
            var keyframe = Keyframe;
            
            _leftHandle.SetTangent(keyframe.inTangent);
            _rightHandle.SetTangent(keyframe.outTangent);
            
            float? inWeight = keyframe.weightedMode is WeightedMode.In or WeightedMode.Both ? keyframe.inWeight : null;
            float? outWeight = keyframe.weightedMode is WeightedMode.Out or WeightedMode.Both ? keyframe.outWeight : null;
            _leftHandle.SetWeight(inWeight, GetXDistInScreen(Left, this));
            _rightHandle.SetWeight(outWeight, GetXDistInScreen(this, Right));
            return;
            
            float GetXDistInScreen(ControlPoint left, ControlPoint right)
            {
                if (left == null || right == null) return 0f;
                var screesPosLeft = _coordinateConverter.GetScreenPosFromCurvePos(left.Keyframe.GetPosition());
                var screesPosRight = _coordinateConverter.GetScreenPosFromCurvePos(right.Keyframe.GetPosition());
                return screesPosRight.x - screesPosLeft.x;
            }
        }
        
        
        private void OnPointerDown(PointerDownEvent evt)
        {
            var elementPosition = new Vector2(resolvedStyle.left, resolvedStyle.top);
            _pointerDownPositionToElementOffset = elementPosition - (Vector2)evt.position; 
            _onPointSelected?.Invoke(this);
            
            switch (evt.button)
            {
                case 0:
                    RegisterCallback<PointerMoveEvent>(OnPointerMove);
                    RegisterCallback<PointerUpEvent>(OnPointerUp);
                    evt.StopPropagation();
                    this.CaptureMouse();
                    
                    _parameterPopup.Show();
                    _parameterPopup.Update(elementPosition, Keyframe);
                    break;
                case 1:
                    _popupMenuController.Show(evt.position, this);
                    evt.StopPropagation();
                    break;
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            var screenPosition = (Vector2)evt.position + _pointerDownPositionToElementOffset;
            
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