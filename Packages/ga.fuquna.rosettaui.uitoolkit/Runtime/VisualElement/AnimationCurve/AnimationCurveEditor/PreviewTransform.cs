using System;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class PreviewTransform : ICoordinateConverter
    {
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

        public void AdjustZoom(Vector2 amount, Vector2 zoomCenterInCurve)
        {
            _zoom /= amount;
            _offset = zoomCenterInCurve - (zoomCenterInCurve - _offset) * amount;
        }

        public void FitToRect(in Rect rect)
        {
            _offset = rect.min;
            _zoom = new Vector2(1f / rect.width, 1f / rect.height);
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
        
        public Rect GetPreviewRect()
        {
            var rect = new Rect
            {
                min = GetCurvePosFromScreenUv(Vector2.zero),
                max = GetCurvePosFromScreenUv(Vector2.one)
            };
            return rect;
        }

        #endregion

    }
}