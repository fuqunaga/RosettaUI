using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class ControlPointHandle : VisualElement
    {
        public enum LeftOrRight
        {
            Left = -1,
            Right = 1
        }
        
        public delegate void OnTangentChanged(float tangent, float weight);
        
        private const float DefaultLineLength = 50f;
        private const string HandleRootClassName = "rosettaui-animation-curve-editor__control-point-handle";
        private const string HandleLineClassName = "rosettaui-animation-curve-editor__control-point-handle__line";
        private const string HandleContainerClassName = "rosettaui-animation-curve-editor__control-point-handle__handle-container";
        private const string HandleContainerWeightClassName = "rosettaui-animation-curve-editor__control-point-handle__handle-container-weight";
        private const string HandleClassName = "rosettaui-animation-curve-editor__control-point-handle__handle";

        private readonly LeftOrRight _leftOrRight;
        private readonly ICoordinateConverter _coordinateConverter;
        private readonly OnTangentChanged _onTangentChanged;

        private VisualElement _lineElement;
        private VisualElement _handleContainerElement;
        
        private float _xDistToNeighborInScreen;
        
        private float Sign => (int)_leftOrRight;
        
        public ControlPointHandle(LeftOrRight leftOrRight, ICoordinateConverter coordinateConverter, OnTangentChanged onTangentChanged)
        {
            _leftOrRight = leftOrRight;
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
            var tangent = _leftOrRight == LeftOrRight.Left ? keyframe.inTangent : keyframe.outTangent;
            var degree = SetTangent(tangent);
            
            var isWeightEnable = keyframe.weightedMode == WeightedMode.Both || 
                (keyframe.weightedMode == WeightedMode.In && _leftOrRight == LeftOrRight.Left) ||
                (keyframe.weightedMode == WeightedMode.Out && _leftOrRight == LeftOrRight.Right);
            
            float? weight = isWeightEnable
                ? (_leftOrRight == LeftOrRight.Left ? keyframe.inWeight : keyframe.outWeight)
                : null;
            
            _xDistToNeighborInScreen = xDistToNeighborInScreen;
            SetWeight(weight, degree);
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
                radian = tangent > 0 ? Mathf.PI / 2 : -Mathf.PI / 2;
            }
            else
            {
                radian = Mathf.Atan(_coordinateConverter.GetScreenTangentFromCurveTangent(tangent));

                if (_leftOrRight == LeftOrRight.Left)
                {
                    radian += Mathf.PI;
                }
            }

            // USSでは時計回りが正方向なので反転する
            _lineElement.style.rotate = new Rotate(Angle.Radians(-radian));

            // ラインの先端のハンドル部分は回転させないよう逆回転しておく
            _handleContainerElement.style.rotate = new Rotate(Angle.Radians(radian));

            return radian * Mathf.Rad2Deg;
        }
        
        private void SetWeight(float? weight, float degree)
        {
            var width = DefaultLineLength;
            if (weight is {} w && !Mathf.Approximately((degree+90) % 180, 0f)) // +-90度付近は無視
            {
                width = Mathf.Max(10f, w * Mathf.Abs(_xDistToNeighborInScreen / Mathf.Cos(degree * Mathf.Deg2Rad)));
            }
            
            _lineElement.style.width = width;
            
            if (weight.HasValue)
            {
                _handleContainerElement.AddToClassList(HandleContainerWeightClassName);
            }
            else
            {
                _handleContainerElement.RemoveFromClassList(HandleContainerWeightClassName);
            }
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