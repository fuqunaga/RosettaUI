using System;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Previewの座標変換を行うクラス
    /// 主に三つの座標系がある
    /// - Curve座標系: アニメーションカーブの座標系
    /// - Screen座標系: UI上のピクセル数ベースの座標系
    /// - ScreenUV座標系: Screen座標系を0~1の範囲に正規化した座標系
    /// UI座標系はそのまま表示されるピクセル数とは異なり、さらにUIDocument全体にスケーリングがかかる
    /// </summary>
    public class PreviewTransform : ICoordinateConverter
    {
        public static float ZoomMin { get; set; } = 1e-10f;
        public static float ZoomMax { get; set; } = 1e+4f;
        
        public Vector2 Zoom => _zoom;
        public Vector2 Offset => _offset;
        public Vector4 OffsetZoom => new(_offset.x, _offset.y, _zoom.x, _zoom.y);

        private Vector2 _zoom = Vector2.one;
        private Vector2 _offset = Vector2.zero;

        private readonly Func<float> _getPreviewWidth;
        private readonly Func<float> _getPreviewHeight;

        public PreviewTransform(Func<float> getPreviewWidth, Func<float> getPreviewHeight)
        {
            _getPreviewWidth = getPreviewWidth;
            _getPreviewHeight = getPreviewHeight;
        }

        public void AdjustOffset(Vector2 amount)
        {
            _offset += amount;
        }

        public void AdjustOffsetByScreenDelta(Vector2 screenDelta)
        {
            AdjustOffset(new Vector2(-screenDelta.x / _getPreviewWidth(), screenDelta.y /_getPreviewHeight()) / _zoom);
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
            var widthOnScreen = _getPreviewWidth();
            var heightOnScreen = _getPreviewHeight();

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

        #region Coordinate Convertion

        public Vector2 GetScreenUvFromScreenPos(Vector2 screenPos)
        {
            return new Vector2(screenPos.x / _getPreviewWidth(), 1f - screenPos.y /_getPreviewHeight());
        }
        
        public Vector2 GetScreenPosFromScreenUv(Vector2 screenUv)
        {
            return new Vector2(screenUv.x * _getPreviewWidth(), (1f - screenUv.y) *_getPreviewHeight());
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
            float aspect = _getPreviewWidth() / _getPreviewHeight() * _zoom.x / _zoom.y;
            tangent *= aspect;
            return tangent;
        }
        
        public float GetScreenTangentFromCurveTangent(float tangent)
        {
            float aspect = _getPreviewWidth() / _getPreviewHeight() * _zoom.x / _zoom.y;
            tangent /= aspect;
            return tangent;
        }

        public Rect PreviewRect
        {
            get
            {
                var rect = new Rect
                {
                    min = GetCurvePosFromScreenUv(Vector2.zero),
                    max = GetCurvePosFromScreenUv(Vector2.one)
                };
                return rect;
            }
        }

        public GridViewport PreviewGridViewport => new(PreviewRect);

        #endregion

    }
}