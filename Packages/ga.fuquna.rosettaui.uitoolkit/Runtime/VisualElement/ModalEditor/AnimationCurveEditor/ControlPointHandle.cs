using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class ControlPointHandle : VisualElement
    {
        private const float DefaultLineLength = 50f;
        private const string HandleRootClassName = "rosettaui-animation-curve-editor__control-point-handle";
        private const string HandleLineClassName = "rosettaui-animation-curve-editor__control-point-handle__line";
        private const string HandleContainerClassName = "rosettaui-animation-curve-editor__control-point-handle__handle-container";
        private const string HandleContainerWeightClassName = "rosettaui-animation-curve-editor__control-point-handle__handle-container-weight";
        private const string HandleClassName = "rosettaui-animation-curve-editor__control-point-handle__handle";

        private readonly InOrOut _inOrOut;
        private readonly ICoordinateConverter _coordinateConverter;
        private readonly Action<float, float> _onTangentChanged;

        private VisualElement _lineElement;
        private VisualElement _handleContainerElement;
        
        private float _xDistToNeighborInScreen;
        
        private float Sign => _inOrOut == InOrOut.In ? -1f : 1f;
        
        public ControlPointHandle(InOrOut inOrOut, ICoordinateConverter coordinateConverter, Action<float, float> onTangentChanged)
        {
            _inOrOut = inOrOut;
            _coordinateConverter = coordinateConverter;
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

        public void UpdateView(in Keyframe keyframe, float xDistToNeighborInScreen)
        {
            var radian = SetTangent(keyframe.GetTangent(_inOrOut));

            float? weight = keyframe.IsWeighted(_inOrOut)
                ? keyframe.GetWeight(_inOrOut)
                : null;
            
            _xDistToNeighborInScreen = xDistToNeighborInScreen;
            SetWeight(weight, radian);
        }
        
        /// <summary>
        /// Tangentを表示に反映する
        /// </summary>
        /// <returns>ラインの角度を返す。USSとは回転方向が異なりX軸＋方向が0度、Y軸が＋90度</returns>
        private float SetTangent(float tangent)
        {
            float radian;

            // tangentが無限大の場合は+-90度にする
            // LeftOrRight.Left側ではハンドルを上に上げて行くとtangentが大きい負の値になるが、無限大になると正の無限大に変わる
            // これはUnityのAnimationCurveEditorの仕様に合わせている
            if (float.IsInfinity(tangent))
            {
                radian = Mathf.Sign(tangent) * Mathf.PI * 0.5f;
            }
            else
            {
                radian = Mathf.Atan(_coordinateConverter.GetScreenTangentFromCurveTangent(tangent));

                if (_inOrOut == InOrOut.In)
                {
                    radian += Mathf.PI;
                }
            }

            // USSでは時計回りが正方向なので反転する
            _lineElement.style.rotate = new Rotate(Angle.Radians(-radian));

            // ラインの先端のハンドル部分は回転させないよう逆回転しておく
            _handleContainerElement.style.rotate = new Rotate(Angle.Radians(radian));

            return radian;
        }
        
        private void SetWeight(float? weight, float radian)
        {
            var width = DefaultLineLength;
            if (weight is {} w
                && !Mathf.Approximately((radian + Mathf.PI * 0.5f) % Mathf.PI, 0f)) // +-90度付近は無視
            {
                width = Mathf.Max(10f, w * Mathf.Abs(_xDistToNeighborInScreen / Mathf.Cos(radian)));
            }
            
            _lineElement.style.width = width;
            
            _handleContainerElement.EnableInClassList(HandleContainerWeightClassName, weight.HasValue);
        }

        #region Pointer Event
        
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

            var deltaY = -(mousePoint.y - centerPoint.y);
            var deltaX = Sign * Mathf.Max(0, Sign * (mousePoint.x - centerPoint.x)); // centerPoint.Xを越えないようにClamp
            
            // deltaXが0に近い場合は無限大にする
            // LeftOrRight.Left側ではハンドルを上に上げて行くとtangentが大きい負の値になるが、無限大になると正の無限大に変わる
            // これはUnityのAnimationCurveEditorの仕様に合わせている
            var tangent = Mathf.Approximately(deltaX, 0f) 
                ? Mathf.Sign(deltaY) * float.PositiveInfinity 
                : deltaY / deltaX;
            
            tangent = _coordinateConverter.GetCurveTangentFromScreenTangent(tangent);

            var weight = Mathf.Clamp01(Mathf.Abs(mousePoint.x - centerPoint.x) / Mathf.Abs(_xDistToNeighborInScreen));
            _onTangentChanged?.Invoke(tangent, weight);
            
            evt.StopPropagation();
        }
        
        private void OnPointerUp(PointerUpEvent evt)
        {
            _handleContainerElement.ReleaseMouse();
            _handleContainerElement.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            evt.StopPropagation();
        }
        
        #endregion
    }
}