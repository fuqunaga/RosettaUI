using System.Collections.Generic;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class AnimationCurveVisualElementHelper
    {
        private static readonly HashSet<VisualElement> TextureAttachedElements = new();

        public static void UpdatePreviewToBackgroundImage(
            AnimationCurve curve,
            VisualElement visualElement,
            AnimationCurvePreviewRenderer.CurvePreviewViewInfo? viewInfoOrNull = null
        )
        {
            var width = Mathf.CeilToInt(visualElement.CalcWidthPixelOnScreen());
            var height = Mathf.CeilToInt(visualElement.CalcHeightPixelOnScreen());
                
            if (width <= 0 || height <= 0)
            {
                return;
            }

            if (viewInfoOrNull is not {} viewInfo)
            {
                var rect = curve.GetCurveRect();
                rect = AnimationCurveHelper.AdjustCurveRectHeightLikeInspector(rect);

                viewInfo = new AnimationCurvePreviewRenderer.CurvePreviewViewInfo
                {
                    offsetZoom = new Vector4(rect.xMin, rect.yMin, 1f / rect.width, 1f / rect.height)
                };
            }


            var renderTexture = visualElement.resolvedStyle.backgroundImage.renderTexture;
            
            var textureGenerated = AnimationCurveHelper.GenerateOrUpdatePreviewTexture(curve, ref renderTexture, width, height, viewInfo);
            
            // 新しいテクスチャを設定
            if (textureGenerated)
            {
                visualElement.style.backgroundImage = Background.FromRenderTexture(renderTexture);

                // 新しいテクスチャをアタッチしたらパネルからのデタッチ時にテクスチャを解放する
                if (!TextureAttachedElements.Contains(visualElement) && renderTexture != null)
                {
                    TextureAttachedElements.Add(visualElement);
                    visualElement.RegisterCallback<DetachFromPanelEvent>(_ => ReleaseAttachedTexture(visualElement));
                }
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