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
        private enum MoveAxis
        {
            Both,
            Horizontal,
            Vertical
        }
        
        private const string ContainerClassName = "rosettaui-animation-curve-editor__control-point-container";
        private const string ControlPointClassName = "rosettaui-animation-curve-editor__control-point";
        private const string ActiveControlPointClassName = "rosettaui-animation-curve-editor__control-point--active";

        // Weightの固定値は1/3の模様
        // https://github.com/Unity-Technologies/UnityCsReference/blob/4b463aa72c78ec7490b7f03176bd012399881768/Editor/Mono/Animation/AnimationWindow/CurveMenuManager.cs#L193
        // Ferguson/Coons曲線をベジエ曲線にするときの係数
        // https://olj611.hatenablog.com/entry/2023/12/01/150842
        private const float WeightDefaultValue = 1 / 3f;


        private readonly Func<(bool, bool)> _getSnapXY;

        private readonly CurveController _curveController;
        private readonly PreviewTransform _previewTransform;
        private readonly ControlPointDisplayPositionPopup _controlPointDisplayPositionPopup;
        private readonly ControlPointEditPositionPopup _controlPointEditPositionPopup;
        private readonly VisualElement _controlPoint;
        private readonly ControlPointHandle _leftHandle;
        private readonly ControlPointHandle _rightHandle;
        private readonly WrapModeButton _wrapModeButton;


        private Vector2 _pointerDownPositionToElementOffset;
        private MoveAxis _pointerMoveAxis = MoveAxis.Both;
        private Vector2 _keyframePositionOnPointerDown;


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
        
        
        public ControlPoint(CurveController curveController, PreviewTransform previewTransform,
            ControlPointDisplayPositionPopup controlPointDisplayPositionPopup, ControlPointEditPositionPopup controlPointEditPositionPopup,
            Func<(bool, bool)> getSnapXY)
        {
            _curveController = curveController;
            _previewTransform = previewTransform;
            _controlPointDisplayPositionPopup = controlPointDisplayPositionPopup;
            _controlPointEditPositionPopup = controlPointEditPositionPopup;
            _getSnapXY = getSnapXY;

            // Control point container
            AddToClassList(ContainerClassName);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            
            // Handles
            _leftHandle = new ControlPointHandle(InOrOut.In,
                _previewTransform,
                (tangent, weight) => OnTangentAndWeightChanged(InOrOut.In, tangent, weight)
            
            );
            Add(_leftHandle);
            
            _rightHandle = new ControlPointHandle(InOrOut.Out,
                _previewTransform,
                (tangent, weight) => OnTangentAndWeightChanged(InOrOut.Out, tangent, weight)
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


            void OnTangentAndWeightChanged(InOrOut inOrOut, float tangent, float weight)
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
            var uiPosition = _previewTransform.GetScreenPosFromCurvePos(KeyframePosition);
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
            var hasLeft = Left != null;
            var hasRight = Right != null;
  
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (!hasLeft && hasRight)
            {
                _wrapModeButton.Show(WrapModeButton.PreOrPost.Pre);
            }
            else if (hasLeft && !hasRight)
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
                var screesPosLeft = _previewTransform.GetScreenPosFromCurvePos(left.Keyframe.GetPosition());
                var screesPosRight = _previewTransform.GetScreenPosFromCurvePos(right.Keyframe.GetPosition());
                return screesPosRight.x - screesPosLeft.x;
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
        
        
        #region Pointer Events
        
        private void OnPointerDown(PointerDownEvent evt)
        {
            var elementPosition = this.GetLocalPosition();
            _pointerDownPositionToElementOffset = elementPosition - (Vector2)evt.position; 
            
            switch (evt.button)
            {
                // Left click
                // - IsActive
                //   - subKey: unselect
                // - !IsActive:
                //   - subKey: add select
                //   - !subKey: select only this. start drag.
                case 0:
                    var subKey = evt.shiftKey || evt.ctrlKey || evt.commandKey;

                    if (IsActive && subKey)
                    {
                        _curveController.UnselectControlPoint(this);
                    }
                    else if(subKey)
                    {
                        _curveController.SelectControlPoint(this, preserveOtherSelection: true);
                    }
                    else
                    {
                        _curveController.SelectControlPoint(this);
                        _pointerMoveAxis = MoveAxis.Both;
                        _keyframePositionOnPointerDown = KeyframePosition;
                        this.CaptureMouse();
                        RegisterCallback<PointerMoveEvent>(OnPointerMove);
                        RegisterCallback<PointerUpEvent>(OnPointerUp);

                        _controlPointDisplayPositionPopup.Show();
                        _controlPointDisplayPositionPopup.Update(elementPosition, KeyframePosition);
                    }

                    evt.StopPropagation();
                    
                    break;
                case 1:
                    _curveController.SelectControlPoint(this);
                    ControlPointPopupMenu.Show(evt.position, this);
                    evt.StopPropagation();
                    break;
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_pointerMoveAxis == MoveAxis.Both && evt.shiftKey)
            {
                var delta = evt.deltaPosition;
                _pointerMoveAxis = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? MoveAxis.Horizontal : MoveAxis.Vertical;
            }
            
            var screenPosition = (Vector2)evt.position + _pointerDownPositionToElementOffset;
            var keyframePosition = _previewTransform.GetCurvePosFromScreenPos(screenPosition);
            
            switch (_pointerMoveAxis)
            {
                case MoveAxis.Both:
                    break;
                case MoveAxis.Horizontal:
                    keyframePosition.y = _keyframePositionOnPointerDown.y;
                    break;
                case MoveAxis.Vertical:
                    keyframePosition.x = _keyframePositionOnPointerDown.x;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var (snapX, snapY) = _getSnapXY();
            if (snapX || snapY)
            {
                var gridViewport = _previewTransform.PreviewGridViewport;
                if (snapX) keyframePosition.x = gridViewport.RoundX(keyframePosition.x, 0.05f);
                if (snapY) keyframePosition.y = gridViewport.RoundY(keyframePosition.y, 0.05f);
            }
            
            KeyframePosition = keyframePosition;
            
            var pointPosition = _previewTransform.GetScreenPosFromCurvePos(KeyframePosition);
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
        
        #endregion

    }
}