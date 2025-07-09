using System.Collections.Generic;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class AnimationCurveVisualElementHelper
    {
        private static readonly HashSet<VisualElement> TextureAttachedElements = new();
        
        public static void UpdateGradientPreviewToBackgroundImage(AnimationCurve curve, VisualElement visualElement)
        {
            var width = Mathf.CeilToInt(visualElement.CalcWidthPixelOnScreen());
            var height = Mathf.CeilToInt(visualElement.CalcHeightPixelOnScreen());
                
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var renderTexture = visualElement.resolvedStyle.backgroundImage.renderTexture;
            var newRenderTexture = AnimationCurveHelper.GenerateOrUpdatePreviewTexture(curve, renderTexture, width, height);

            visualElement.style.backgroundImage = Background.FromRenderTexture(newRenderTexture);
            
            // 新しいテクスチャをアタッチしたらパネルからのデタッチ時にテクスチャを解放する
            if (!TextureAttachedElements.Contains(visualElement) && newRenderTexture != null && renderTexture != newRenderTexture)
            {
                TextureAttachedElements.Add(visualElement);
                visualElement.RegisterCallback<DetachFromPanelEvent>(_ => ReleaseAttachedTexture(visualElement));
            }

            return;
            
            static void ReleaseAttachedTexture(VisualElement element)
            {
                var renderTexture = element.resolvedStyle.backgroundImage.renderTexture;
                if (renderTexture != null && renderTexture.IsCreated())
                {
                    renderTexture.Release();
                    Object.Destroy(renderTexture);
                }
                
                TextureAttachedElements.Remove(element);
            }
        }
    }
}