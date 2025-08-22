using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Parameter display popup shown when moving a ControlPoint
    /// </summary>
    public class ControlPointDisplayPositionPopup : Label
    {
        private const string ParameterPopupClassName = "rosettaui-animation-curve-editor__control-point-display-position-popup";
        
        public Vector2 positionOffset = new(2f, -2f);
        private readonly PreviewTransform _previewTransform;


        public ControlPointDisplayPositionPopup(PreviewTransform previewTransform)
        {
            _previewTransform = previewTransform;
            
            AddToClassList(ParameterPopupClassName);
            Hide();
        }
        
        public void Show()
        {
            style.display = DisplayStyle.Flex;
        }
        
        public void Hide()
        {
            style.display = DisplayStyle.None;
        }
        
        public void Update(Vector2 positionLeftTop, Vector2 keyframePosition)
        {
            SetKeyframePosition(keyframePosition);
            SetPosition(positionLeftTop);
        }

        public void SetKeyframePosition(Vector2 keyframePosition)
        {
            const int maxPrecision = 4;
            var gridViewport = _previewTransform.PreviewGridViewport;
            var orderX = Mathf.Max(0, maxPrecision - Mathf.CeilToInt(gridViewport.XOrder));
            var orderY = Mathf.Max(0, maxPrecision - Mathf.CeilToInt(gridViewport.YOrder));
            
            var timeString = keyframePosition.x.ToString($"N{orderX}");
            var valueString = keyframePosition.y.ToString($"N{orderY}");
            
            text = $"{timeString}, {valueString}";
        }
        
        public void SetPosition(Vector2 positionLeftTop)
        {
            var positionWithOffset = positionLeftTop + positionOffset;
            style.left = positionWithOffset.x;
            style.top = positionWithOffset.y;

            RegisterCallbackOnce<GeometryChangedEvent>(_ =>
            {
                this.ClampElementToParent();
            });
        }
    }
}