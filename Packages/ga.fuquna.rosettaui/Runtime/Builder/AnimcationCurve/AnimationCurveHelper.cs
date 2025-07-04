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
        
        public static Rect GetCurveRect(this AnimationCurve curve, bool clamp = false, bool adjustForVisibility = false, int stepNum = 64)
        {
            var rect = new Rect();
            if (curve.keys.Length <= 0) return rect;
            bool isWithin01 = true;
            for (var i = 0; i < curve.keys.Length; i++)
            {
                var keyframe = curve.keys[i];
                isWithin01 &= keyframe.time is >= 0f and <= 1f;
                if (i == 0)
                {
                    rect.xMin = keyframe.time;
                    rect.xMax = keyframe.time;
                    continue;
                }
                rect.xMin = Mathf.Min(rect.xMin, keyframe.time);
                rect.xMax = Mathf.Max(rect.xMax, keyframe.time);
            }
            
            for (int i = 0; i < stepNum; i++)
            {
                float y = curve.Evaluate(rect.xMin + i / (stepNum - 1f) * rect.width);
                if (i == 0)
                {
                    rect.yMin = y;
                    rect.yMax = y;
                    continue;
                }
                rect.yMin = Mathf.Min(rect.yMin, y);
                rect.yMax = Mathf.Max(rect.yMax, y);
            }

            if (adjustForVisibility)
            {
                if (isWithin01)
                {
                    rect.xMin = 0f;
                    rect.xMax = 1f;
                }

                if (rect.yMin is > 0f and <= 1f) { rect.yMin = 0f; }
                if (rect.yMax is < 1f and >= 0f) { rect.yMax = 1f; }
            }
            
            if (clamp)
            {
                if (rect.width <= 0f)
                {
                    rect.width = 1f;
                    rect.xMin -= 1f;
                }
                if (rect.height <= 0f)
                {
                    rect.height = 1f;
                    rect.yMin -= 1f;
                }
            }
            return rect;
        }
        
        public static void UpdateAnimationCurvePreviewToBackgroundImage(AnimationCurve curve, VisualElement visualElement)
        {
            var width = (int)visualElement.resolvedStyle.width;
            var height = (int)visualElement.resolvedStyle.height;
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

            var curveRect = curve.GetCurveRect(true);
            var colorArray = ArrayPool<Color>.Shared.Rent(width * height);

            // Fill the texture with a gray color
            for (int j = 0; j < width * height; j++)
                colorArray[j] = new Color(0.3372549f, 0.3372549f, 0.3372549f, 0.5f);

            int prevHeight = -1;
            for (int i = 0; i < width; i++)
            {
                float t = curveRect.xMin + i / (width - 1f) * curveRect.width;
                int targetHeight = (int)((curve.Evaluate(t) - curveRect.yMin) / curveRect.height * height);

                if (prevHeight >= 0)
                {
                    for (int j = Mathf.Min(prevHeight, targetHeight); j <= Mathf.Max(prevHeight, targetHeight); j++)
                    {
                        if (j >= 0 && j < height) colorArray[j * width + i] = Color.green;
                    }
                }
                prevHeight = targetHeight;
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
