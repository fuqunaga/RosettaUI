using System.Buffers;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RosettaUI.Builder
{
    public static class AnimationCurveHelper 
    {
        public static AnimationCurve Clone(AnimationCurve src)
        {
            var dst = new AnimationCurve();
            Copy(src, dst);
            return dst;
        }
        
        public static void Copy(AnimationCurve src, AnimationCurve dst)
        {
            dst.keys = src.keys;
            dst.postWrapMode = src.postWrapMode;
            dst.preWrapMode = src.preWrapMode;
        }
        
        public static Rect GetCurveRect(this AnimationCurve curve, int stepNum = 64)
        {
            var rect = new Rect();
            foreach (var keyframe in curve.keys)
            {
                rect.xMin = Mathf.Min(rect.xMin, keyframe.time);
                rect.xMax = Mathf.Max(rect.xMax, keyframe.time);
            }
            for (int i = 0; i < stepNum; i++)
            {
                float y = curve.Evaluate(i / (stepNum - 1f));
                rect.yMin = Mathf.Min(rect.yMin, y);
                rect.yMax = Mathf.Max(rect.yMax, y);
            }
            return rect;
        }
        
        public static void UpdateAnimationCurvePreviewToBackgroundImage(AnimationCurve curve, VisualElement visualElement)
        {
            int width = (int)(visualElement.resolvedStyle.width * 1.5f);
            int height = (int)(visualElement.resolvedStyle.height * 1.5f);
            if (width <= 0 || height <= 0) return;
            
            var style = visualElement.style;
            var texture = style.backgroundImage.value.texture;
            style.backgroundImage = GenerateAnimationCurvePreview(curve, texture, width, height);
        }
        
        public static Texture2D GenerateAnimationCurvePreview(AnimationCurve curve, Texture2D texture, int width = 256, int height = 32)
        {
            if (texture != null && (texture.width != width || texture.height != height))
            {
                Object.DestroyImmediate(texture);
                texture = null;
            }
            
            var curveRect = curve.GetCurveRect();
            var colorArray = ArrayPool<Color>.Shared.Rent(width * height);
            for (int i = 0; i < width; i++)
            {
                int targetHeight = (int)((curve.Evaluate(curveRect.xMin + i / (width - 1f) * curveRect.width) - curveRect.yMin) / curveRect.height * height);
                for (int j = 0; j < height; j++)
                {
                    colorArray[j * width + i] = j == targetHeight ? Color.green : new Color(0.3372549f, 0.3372549f, 0.3372549f);
                }
            }
            
            texture ??= new Texture2D(width, height)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.SetPixels(0, 0, width, height, colorArray);
            texture.Apply();
            
            ArrayPool<Color>.Shared.Return(colorArray);
            
            return texture;
        }
    }
}
