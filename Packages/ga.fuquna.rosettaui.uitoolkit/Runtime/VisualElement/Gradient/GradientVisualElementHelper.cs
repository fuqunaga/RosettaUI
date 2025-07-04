using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class GradientVisualElementHelper
    {
        public static void UpdateGradientPreviewToBackgroundImage(Gradient gradient, VisualElement visualElement)
        {
            var style = visualElement.style;
            var texture = style.backgroundImage.value.texture;
            style.backgroundImage = GradientHelper.GenerateGradientPreview(gradient, texture);
        }
    }
}