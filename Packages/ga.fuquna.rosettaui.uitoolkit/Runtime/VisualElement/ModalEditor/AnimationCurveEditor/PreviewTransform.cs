using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Previewの座標変換を行うクラス
    /// 主に三つの座標系がある
    /// - Curve座標系: アニメーションカーブの座標系
    /// - Screen座標系: PreviewElement上のピクセル数ベースの座標系
    /// - ScreenUV座標系: Screen座標系を0~1の範囲に正規化した座標系
    /// - UIWorld座標系: UI Document全体の座標系
    /// UI座標系はそのまま表示されるピクセル数とは異なり、さらにUIDocument全体にスケーリングがかかる
    /// </summary>
    public class PreviewTransform : ICoordinateConverter
    {
        public static float ZoomMin { get; set; } = 1e-10f;
        public static float ZoomMax { get; set; } = 1e+4f;
        
        
        private Vector2 _zoom = Vector2.one;
        private Vector2 _offset = Vector2.zero;
        
        private readonly VisualElement _previewElement;
        private readonly Func<(bool, bool)> _getSnapEnable;

        
        public Vector2 Zoom => _zoom;
        public Vector2 Offset => _offset;
        public Vector4 OffsetZoom => new(_offset.x, _offset.y, _zoom.x, _zoom.y);
        
        public Rect PreviewRect => new()
        {
            min = GetCurvePosFromScreenUv(Vector2.zero),
            max = GetCurvePosFromScreenUv(Vector2.one)
        };

        public GridViewport PreviewGridViewport => new(PreviewRect);

        private Vector2 PreviewSize
        {
            get
            {
                var previewStyle = _previewElement.resolvedStyle;
                return new Vector2(previewStyle.width, previewStyle.height);
            }
        }

        
        public PreviewTransform(VisualElement previewElement, Func<(bool, bool)> getSnapEnable)
        {
            _previewElement = previewElement;
            _getSnapEnable = getSnapEnable;
        }

        public void AdjustOffset(Vector2 amount)
        {
            _offset += amount;
        }

        public void AdjustOffsetByScreenDelta(Vector2 screenDelta)
        {
            var previewSize = PreviewSize;
            AdjustOffset(new Vector2(-screenDelta.x / previewSize.x, screenDelta.y / previewSize.y) / _zoom);
        }

        public void SetXCenter(float xCenter)
        {
            _offset.x = xCenter - 0.5f / _zoom.x;
        }

        public void SetYCenter(float yCenter)
        {
            _offset.y = yCenter - 0.5f / _zoom.y;
        }

        public void AdjustZoom(Vector2 amount, Vector2 zoomCenterOnCurve)
        {
            var prevZoom = _zoom;
            _zoom /= amount;
            var zoomIsInRange = _zoom.x >= ZoomMin && _zoom.x <= ZoomMax && _zoom.y >= ZoomMin && _zoom.y <= ZoomMax;
            if (!zoomIsInRange)
            {
                _zoom = prevZoom;
                return;
            }
            
            _offset = zoomCenterOnCurve - (zoomCenterOnCurve - _offset) * amount;
        }

        public void FitToRect(Rect rectOnCurve, RectOffset paddingOnScreen)
        {
            var widthOnCurve = rectOnCurve.width;
            var heightOnCurve = rectOnCurve.height;
            
            var previewSize = PreviewSize;
            var widthOnScreen = previewSize.x;
            var heightOnScreen = previewSize.y;

            rectOnCurve.xMin -= GetCurveLengthFromScreenLength(widthOnCurve, widthOnScreen, paddingOnScreen.left);
            rectOnCurve.xMax += GetCurveLengthFromScreenLength(widthOnCurve, widthOnScreen, paddingOnScreen.right);
            rectOnCurve.yMin -= GetCurveLengthFromScreenLength(heightOnCurve, heightOnScreen, paddingOnScreen.bottom);
            rectOnCurve.yMax += GetCurveLengthFromScreenLength(heightOnCurve, heightOnScreen, paddingOnScreen.top);
            
            _offset = rectOnCurve.min;
            _zoom = new Vector2(1f / rectOnCurve.width, 1f / rectOnCurve.height);
            
            return;


            float GetCurveLengthFromScreenLength(float rangeOnCurve, float rangeOnScreen, float lengthOnScreen)
            {
                // 次の方程式を式変形して
                // lengthOnCurve / (rangeOnCurve + lengthOnCurve * 2) = lengthOnScreen / rangeOnScreen;  
                // 次の式を得る
                // lengthOnCurve = (lengthOnScreen * rangeOnCurve) / (rangeOnScreen - lengthOnScreen * 2);
                
                return Mathf.Max(0f, (lengthOnScreen * rangeOnCurve) / (rangeOnScreen - lengthOnScreen * 2));
            }
        }
        
        public Vector2 SnapCurvePositionIfEnabled(Vector2 curvePos)
        {
            var gridViewport = PreviewGridViewport;
            var (snapX, snapY) = _getSnapEnable();
            if (snapX) curvePos.x = gridViewport.RoundX(curvePos.x, 0.05f);
            if (snapY) curvePos.y = gridViewport.RoundY(curvePos.y, 0.05f);
            return curvePos;
        }

        #region Coordinate Convertion

        public Vector2 GetScreenUvFromScreenPos(Vector2 screenPos)
        {
            var previewSize = PreviewSize;
            return new Vector2(screenPos.x / previewSize.x, 1f - screenPos.y / previewSize.y);
        }
        
        public Vector2 GetScreenPosFromScreenUv(Vector2 screenUv)
        {
            var previewSize = PreviewSize;
            return new Vector2(screenUv.x * previewSize.x, (1f - screenUv.y) * previewSize.y);
        }
        
        public Vector2 GetCurvePosFromScreenUv(Vector2 screenUv)
        {
            return screenUv / _zoom + _offset;
        }
        
        public Vector2 GetScreenUvFromCurvePos(Vector2 curvePos)
        {
            return (curvePos - _offset) * _zoom;
        }

        public float GetCurveTangentFromScreenTangent(float tangent)
        {
            var previewSize = PreviewSize;
            var aspect = previewSize.x / previewSize.y * _zoom.x / _zoom.y;
            tangent *= aspect;
            return tangent;
        }
        
        public float GetScreenTangentFromCurveTangent(float tangent)
        {
            var previewSize = PreviewSize;
            var aspect = previewSize.x / previewSize.y * _zoom.x / _zoom.y;
            tangent /= aspect;
            return tangent;
        }
        
        public Vector2 GetScreenPosFromUIWorldPos(Vector2 uiWorldPos)
        {
            return _previewElement.WorldToLocal(uiWorldPos);
        }
        
        public Vector2 GetUIWorldPosFromScreenPos(Vector2 screenPos)
        {
            return _previewElement.LocalToWorld(screenPos);
        }
        
        #endregion
    }
}