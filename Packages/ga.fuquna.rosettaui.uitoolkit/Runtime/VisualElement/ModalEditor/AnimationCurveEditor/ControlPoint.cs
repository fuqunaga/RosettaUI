using System;
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
        
        private readonly CurveController _curveController;
        private readonly PreviewTransform _previewTransform;
        private readonly VisualElement _controlPoint;
        private readonly ControlPointHandle _leftHandle;
        private readonly ControlPointHandle _rightHandle;
        private readonly WrapModeButton _wrapModeButton;


        private Vector2 _pointerDownPositionToElementOffset;
        private Vector2 _keyframePositionOnPointerDown;


        /// Needs to call CurveController.OnCurveChanged() after this to reflect the change in the view
        public bool IsKeyBroken { get; set; }

        /// Needs to call CurveController.OnCurveChanged() after this to reflect the change in the view
        public TangentMode InTangentMode { get; set; } = TangentMode.Free;
        
        /// Needs to call CurveController.OnCurveChanged() after this to reflect the change in the view
        public TangentMode OutTangentMode { get; set; } = TangentMode.Free;



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
            private set => _curveController.UpdateKeyframes(new []{(this, value)});
        }
        
        public Vector2 KeyframePosition => Keyframe.GetPosition();

        /// <summary>
        /// UIToolkitのLocalPosition
        /// get時、resolveStyleに反映されるのはレイアウト計算後なのでstyle.left/topを直接参照する
        /// </summary>
        public Vector2 LocalPosition
        {
            get => new(style.left.value.value, style.top.value.value);
            private set
            {
                style.left = value.x;
                style.top = value.y;
            }
        }

        private ControlPoint Left => _curveController.GetControlPointLeft(this);
        private ControlPoint Right => _curveController.GetControlPointRight(this);
        
        
        public ControlPoint(CurveController curveController, PreviewTransform previewTransform)
        {
            _curveController = curveController;
            _previewTransform = previewTransform;

            // Control point container
            AddToClassList(ContainerClassName);
            
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
                }

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
        
        public void UpdateView()
        {
            LocalPosition = _previewTransform.GetScreenPosFromCurvePos(KeyframePosition);
            
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
                var screesPosLeft = _previewTransform.GetScreenPosFromCurvePos(left.KeyframePosition);
                var screesPosRight = _previewTransform.GetScreenPosFromCurvePos(right.KeyframePosition);
                return screesPosRight.x - screesPosLeft.x;
            }
        }
    }
}