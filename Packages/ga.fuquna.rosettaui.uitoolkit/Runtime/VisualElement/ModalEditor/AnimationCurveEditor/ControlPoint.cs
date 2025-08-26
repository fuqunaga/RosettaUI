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

        // Weightの固定値は1/3の模様
        // https://github.com/Unity-Technologies/UnityCsReference/blob/4b463aa72c78ec7490b7f03176bd012399881768/Editor/Mono/Animation/AnimationWindow/CurveMenuManager.cs#L193
        // Ferguson/Coons曲線をベジエ曲線にするときの係数
        // https://olj611.hatenablog.com/entry/2023/12/01/150842
        private const float WeightDefaultValue = 1 / 3f;
        
        
        private readonly Action<ControlPoint, Vector2> _onPointMoved;

        private readonly CurveController _curveController;
        private readonly ICoordinateConverter _coordinateConverter;
        private readonly ControlPointDisplayPositionPopup _controlPointDisplayPositionPopup;
        private readonly ControlPointEditPositionPopup _controlPointEditPositionPopup;
        private readonly VisualElement _controlPoint;
        private readonly ControlPointHandle _leftHandle;
        private readonly ControlPointHandle _rightHandle;
        private readonly WrapModeButton _wrapModeButton;


        private Vector2 _pointerDownPositionToElementOffset;


        public bool IsKeyBroken { get; private set; }

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
            get => _curveController.GetKeyframe(this);
            set => _curveController.UpdateKeyframe(this, value);
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
        
        private ControlPoint Left => _curveController.GetControlPointLeft(this);
        private ControlPoint Right => _curveController.GetControlPointRight(this);
        
        private bool IsFirst => Left == null;
        private bool IsLast => Right == null;

        public ControlPoint(CurveController curveController, ICoordinateConverter coordinateConverter,
            ControlPointDisplayPositionPopup controlPointDisplayPositionPopup, ControlPointEditPositionPopup controlPointEditPositionPopup,
            Action<ControlPoint, Vector2> onPointMoved)
        {
            _curveController = curveController;
            _coordinateConverter = coordinateConverter;
            _controlPointDisplayPositionPopup = controlPointDisplayPositionPopup;
            _controlPointEditPositionPopup = controlPointEditPositionPopup;
            _onPointMoved = onPointMoved;

            // Control point container
            AddToClassList(ContainerClassName);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            
            // Handles
            _leftHandle = new ControlPointHandle(ControlPointHandle.LeftOrRight.Left,
                _coordinateConverter,
                (tangent, weight) => OnTangentAndWeightChanged(KeyframeExtensions.InOrOut.In, tangent, weight)
            
            );
            Add(_leftHandle);

            _rightHandle = new ControlPointHandle(ControlPointHandle.LeftOrRight.Right,
                _coordinateConverter,
                (tangent, weight) => OnTangentAndWeightChanged(KeyframeExtensions.InOrOut.Out, tangent, weight)
            );
            Add(_rightHandle);


            // Control point
            _controlPoint = new VisualElement { name = "AnimationCurveEditorControlPoint" };
            _controlPoint.AddToClassList(ControlPointClassName);
            Add(_controlPoint);

            
            // Clamp mode button
            _wrapModeButton = new WrapModeButton();
            _wrapModeButton.RegisterCallback<PointerDownEvent>(_ => OnWrapModeButtonClicked());
            Add(_wrapModeButton);
            
            return;


            void OnTangentAndWeightChanged(KeyframeExtensions.InOrOut inOrOut, float tangent, float weight)
            {
                var keyframe = Keyframe;
                
                keyframe.SetTangent(inOrOut, tangent);
                
                if (!IsKeyBroken)
                {
                    var oppositeTangent = float.IsInfinity(tangent) ? -tangent : tangent;
                    keyframe.SetTangent(inOrOut.Opposite(), oppositeTangent);
                }
                
                keyframe.SetWeight(inOrOut, keyframe.IsWeighted(inOrOut) ? weight : WeightDefaultValue); 


                Keyframe = keyframe;
            }
            
            void OnWrapModeButtonClicked()
            {
                Action<WrapMode> setWrapMode;
                WrapMode currentMode;
                
                switch (_wrapModeButton.CurrentPreOrPost)  
                {
                    case WrapModeButton.PreOrPost.Pre :
                        setWrapMode = _curveController.SetPreWrapMode;
                        currentMode = _curveController.Curve.preWrapMode;
                        break;
                    case WrapModeButton.PreOrPost.Post:
                        setWrapMode = _curveController.SetPostWrapMode;
                        currentMode = _curveController.Curve.postWrapMode;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                };
                

                var position = _wrapModeButton.worldBound.position;
                position.y += 10f;

                PopupMenuUtility.Show(new[]
                    {
                        CreateItem("Loop", WrapMode.Loop),
                        CreateItem("Ping Pong", WrapMode.PingPong),
                        CreateItem("Clamp", WrapMode.ClampForever),
                    },
                    position,
                    _wrapModeButton
                );

                return;

                MenuItem CreateItem(string itemName, WrapMode mode)
                {
                    return new MenuItem(itemName, () => setWrapMode(mode)){ isChecked = mode == currentMode};
                }
            }
        }

        public void SetKeyBroken(bool isBroken, bool updateView = true)
        {
            IsKeyBroken = isBroken;

            if (updateView)
            {
                _curveController.OnCurveChanged();
            }
        }

        public void SetInTangentMode(TangentMode mode, bool updateView = true)
        {
            InTangentMode = mode;
            OnTangentModeChanged(updateView);
        }
        
        public void SetOutTangentMode(TangentMode mode, bool updateView = true)
        {
            OutTangentMode = mode;
            OnTangentModeChanged(updateView);
        }
        
        public void SetBothTangentMode(TangentMode mode, bool updateView = true)
        {
            InTangentMode = mode;
            OutTangentMode = mode;
            OnTangentModeChanged(updateView);
        }
        
        private void OnTangentModeChanged(bool updateView = true)
        {
            SetKeyBroken(true, false);

            if (updateView)
            {
                _curveController.OnCurveChanged();
            }
        }
        
        public void UpdateView()
        {
            var uiPosition = _coordinateConverter.GetScreenPosFromCurvePos(KeyframePosition);
            style.left = uiPosition.x;
            style.top = uiPosition.y;

            UpdateWrapModeButton();

            if (IsActive)
            {
                UpdateHandleView();
            }
        }
        
        private void UpdateWrapModeButton()
        {
            if (IsFirst)
            {
                _wrapModeButton.Show(WrapModeButton.PreOrPost.Pre);
            }
            else if (IsLast)
            {
                _wrapModeButton.Show(WrapModeButton.PreOrPost.Post);
            }
            else
            {
                _wrapModeButton.Hide();
            }
        }

        private void UpdateHandleView()
        {
            var keyframe = Keyframe;

            var leftHandleEnable = IsActive && (Left != null) && (InTangentMode == TangentMode.Free);
            var rightHandleEnable = IsActive && (Right != null) && (OutTangentMode == TangentMode.Free);

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
            _curveController.SelectControlPoint(this);
            
            switch (evt.button)
            {
                case 0:
                    RegisterCallback<PointerMoveEvent>(OnPointerMove);
                    RegisterCallback<PointerUpEvent>(OnPointerUp);
                    evt.StopPropagation();
                    this.CaptureMouse();
                    
                    _controlPointDisplayPositionPopup.Show();
                    _controlPointDisplayPositionPopup.Update(elementPosition, KeyframePosition);
                    break;
                case 1:
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
            var desiredKeyframePosition = _coordinateConverter.GetCurvePosFromScreenPos(screenPosition);
            _onPointMoved(this, desiredKeyframePosition);
            
            var pointPosition = _coordinateConverter.GetScreenPosFromCurvePos(KeyframePosition);
            var popupPosition = pointPosition + _controlPointDisplayPositionPopup.positionOffset;
            _controlPointDisplayPositionPopup.Update(popupPosition, KeyframePosition);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button == 0)
            {
                UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                UnregisterCallback<PointerUpEvent>(OnPointerUp);
                evt.StopPropagation();
                this.ReleaseMouse();
                
                _controlPointDisplayPositionPopup.Hide();
            }
        }

        public void Remove()
        {
            _curveController.RemoveControlPoint(this);
        }

        public void ShowEditKeyPopup()
        {
            _controlPointEditPositionPopup.Show(this);
        }
    }
}