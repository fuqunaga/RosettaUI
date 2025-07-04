using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class AnimationCurveVisualElementHelper
    {
        public static void UpdateGradientPreviewToBackgroundImage(AnimationCurve curve, VisualElement visualElement)
        {
            var width = Mathf.CeilToInt(visualElement.CalcWidthPixelOnScreen());
            var height = Mathf.CeilToInt(visualElement.CalcHeightPixelOnScreen());
                
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var texture = visualElement.style.backgroundImage.value.renderTexture;
            texture = AnimationCurveHelper.GenerateAnimationCurvePreview(curve, texture, width, height);

            visualElement.style.backgroundImage = Background.FromRenderTexture(texture);
        }
    }
}