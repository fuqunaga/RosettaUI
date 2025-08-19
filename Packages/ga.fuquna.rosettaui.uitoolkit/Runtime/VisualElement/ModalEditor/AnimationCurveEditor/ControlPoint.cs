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
        // private readonly ControlPointPopupMenuController _popupMenuController;


        private Vector2 _pointerDownPositionToElementOffset;


        public delegate void OnPointAction(ControlPoint controlPoint);
        
        // インスペクタのアニメーションカーブエディタはPointMode.SmoothかつFlatという状態がある
        // あまりユーザー体験に影響がなさそうなのでここでは排他的な扱いにしている
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
                }
                else
                {
                    _controlPoint.RemoveFromClassList(ActiveControlPointClassName);
                }
                
                UpdateHandleView();
            }
        }

        public Keyframe Keyframe
        {
            get => _holder.GetKeyframe(this);
            set => _holder.UpdateKeyframe(this, value);
        }
        
        public Vector2 KeyframePosition
        {
            get => Keyframe.GetPosition();
            set
            {
                var keyframe = Keyframe;
                keyframe.SetPosition(value);
                Keyframe = keyframe;
            }
        }
        
        private ControlPoint Left => _holder.GetControlPointLeft(this);
        private ControlPoint Right => _holder.GetControlPointRight(this);

        public ControlPoint(ControlPointHolder holder, ICoordinateConverter coordinateConverter,
            ParameterPopup parameterPopup, EditKeyPopup editKeyPopup,
            OnPointAction onPointSelected, Action<ControlPoint, Vector2> onPointMoved)
        {
            _holder = holder;
            _coordinateConverter = coordinateConverter;
            _parameterPopup = parameterPopup;
            _editKeyPopup = editKeyPopup;
            _onPointSelected = onPointSelected;
            _onPointMoved = onPointMoved;

            // Handles
            _leftHandle = new ControlPointHandle(ControlPointHandle.LeftOrRight.Left,
                _coordinateConverter,
                (tangent, weight) =>
                {
                    var keyframe = Keyframe;
                    keyframe.inTangent = tangent;
                    keyframe.inWeight = keyframe.weightedMode is WeightedMode.In or WeightedMode.Both
                        ? weight
                        : 0.333333f;
                    if (PointMode == PointMode.Flat)
                    {
                        PointMode = PointMode.Smooth;
                    }

                    if (PointMode == PointMode.Smooth)
                    {
                        if (float.IsInfinity(keyframe.inTangent)) keyframe.outTangent = -keyframe.inTangent;
                        keyframe.outTangent = keyframe.inTangent;
                    }

                    Keyframe = keyframe;
                }
            );
            Add(_leftHandle);

            _rightHandle = new ControlPointHandle(ControlPointHandle.LeftOrRight.Right,
                _coordinateConverter,
                (tangent, weight) =>
                {
                    var keyframe = Keyframe;
                    keyframe.outTangent = tangent;
                    keyframe.outWeight = keyframe.weightedMode is WeightedMode.Out or WeightedMode.Both
                        ? weight
                        : 0.333333f;
                    if (PointMode == PointMode.Flat)
                    {
                        PointMode = PointMode.Smooth;
                    }

                    if (PointMode == PointMode.Smooth)
                    {
                        if (float.IsInfinity(keyframe.outTangent)) keyframe.inTangent = -keyframe.outTangent;
                        keyframe.inTangent = keyframe.outTangent;
                    }

                    Keyframe = keyframe;
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
            // _popupMenuController = new ControlPointPopupMenuController(
            //     SetPointModeAndUpdateView,
            //     SetTangentModeAndUpdateView,
            //     ToggleWeightedModeAndUpdateView
            // );

            return;


            void SetPointModeAndUpdateView(PointMode mode)
            {
                SetPointMode(mode);
                switch (mode)
                {
                    case PointMode.Broken:
                        break;

                    case PointMode.Flat:
                    {
                        var keyframe = Keyframe;
                        keyframe.inTangent = 0f;
                        keyframe.outTangent = 0f;
                        Keyframe = keyframe;
                    }
                        break;

                    case PointMode.Smooth:
                    {
                        var keyframe = Keyframe;
                        keyframe.inTangent = keyframe.outTangent;
                        Keyframe = keyframe;
                    }
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }
            
            // void SetTangentModeAndUpdateView(TangentMode? inTangentMode, TangentMode? outTangentMode)
            // {
            //     SetTangentMode(inTangentMode, outTangentMode);
            //     UpdateHandleView();
            // }
            
            void ToggleWeightedModeAndUpdateView(WeightedMode mode)
            {
                var keyframe = Keyframe;
                keyframe.ToggleWeightedFrag(mode);
                Keyframe = keyframe;
            }
        }
        
        public void SetPointMode(PointMode mode, bool updateView = true)
        {
            if (PointMode == mode) return;
            
            PointMode = mode;

            if (mode != PointMode.Broken)
            {
                InTangentMode = TangentMode.Free;
                OutTangentMode = TangentMode.Free;
            }

            if (updateView)
            {
                var keyframe = Keyframe;
                keyframe.SetPointMode(mode);
                Keyframe = keyframe;
            }
        }

        public void SetInTangentMode(TangentMode mode, bool updateView = true)
        {
            InTangentMode = mode;
            OnTangentModeChanged(mode, updateView);
        }
        
        public void SetOutTangentMode(TangentMode mode, bool updateView = true)
        {
            OutTangentMode = mode;
            OnTangentModeChanged(mode, updateView);
        }
        
        private void OnTangentModeChanged(TangentMode mode, bool updateView = true)
        {
            PointMode = PointMode.Broken;

            if (updateView)
            {
                _holder.OnCurveChanged();
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

            var leftHandleEnable = IsActive && (Left != null) && (InTangentMode != TangentMode.Linear);
            var rightHandleEnable = IsActive && (Right != null) && (OutTangentMode != TangentMode.Linear);

            Update(leftHandleEnable, _leftHandle, keyframe, GetXDistInScreen(Left, this));
            Update(rightHandleEnable, _rightHandle, keyframe, GetXDistInScreen(this, Right));
            
            return;
            
            
            static void Update(bool enable, ControlPointHandle handle, Keyframe keyframe, float xDistToNeighborInScreen)
            {
                if (!enable)
                {
                    handle.style.visibility = Visibility.Hidden;
                    return;
                }
                
                handle.style.visibility = Visibility.Visible;
                handle.UpdateView(keyframe, xDistToNeighborInScreen);
            }
            
            
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
                    // _popupMenuController.Show(evt.position, this);
                    ControlPointPopupMenu.Show(evt.position, this);
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

        public void Remove()
        {
            _holder.RemoveControlPoint(this);
        }

        public void ShowEditKeyPopup()
        {
            _editKeyPopup.Show(this);
        }
    }
}