using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        
        public static RenderTexture GenerateOrUpdatePreviewTexture(AnimationCurve curve, RenderTexture texture, int width, int height)
        {
            if (texture!= null && (texture.width != width || texture.height != height))
            {
                texture.Release();
                Object.DestroyImmediate(texture);
                texture = null;
            }

            if (texture == null)
            {
                texture = new RenderTexture(width, height, 0)
                {
                    name = "AnimationCurvePreview",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
            }
            
            AnimationCurvePreviewRenderer.Render(curve, new AnimationCurvePreviewRenderer.CurvePreviewViewInfo
            {
                outputTexture = texture,
                resolution = new Vector2(width, height),
                offsetZoom = new Vector4(0f, 0f, 1f, 1f),
                gridParams = new Vector4(0.001f, 0.001f, 0.5f, 0.5f)
            });
            
            return texture;
        }
    }
}
