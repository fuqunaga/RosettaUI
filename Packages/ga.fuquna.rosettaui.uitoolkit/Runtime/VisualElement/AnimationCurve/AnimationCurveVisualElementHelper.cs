using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class AnimationCurveVisualElementHelper
    {
        private static readonly Dictionary<VisualElement, RenderTexture> BackgroundRenderTextureTable = new();
        
        public static void UpdateGradientPreviewToBackgroundImage(AnimationCurve curve, VisualElement visualElement)
        {
            var width = Mathf.CeilToInt(visualElement.CalcWidthPixelOnScreen());
            var height = Mathf.CeilToInt(visualElement.CalcHeightPixelOnScreen());
                
            if (width <= 0 || height <= 0)
            {
                return;
            }
            
            // Remove VisualElements that are no longer in the hierarchy
            foreach(var ve in BackgroundRenderTextureTable.Keys.Where(ve => ve.parent == null))
            {
                BackgroundRenderTextureTable.Remove(ve);
            }
            
            BackgroundRenderTextureTable.TryGetValue(visualElement, out var renderTexture);
            var newRenderTexture = AnimationCurveHelper.GenerateAnimationCurvePreview(curve, renderTexture, width, height);

            if (renderTexture != newRenderTexture)
            {
                visualElement.style.backgroundImage = Background.FromRenderTexture(newRenderTexture);
                BackgroundRenderTextureTable[visualElement] = newRenderTexture;
            }
        }
    }
}