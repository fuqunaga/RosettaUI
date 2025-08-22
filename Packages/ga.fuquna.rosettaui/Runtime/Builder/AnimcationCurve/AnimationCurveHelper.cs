using UnityEngine;

namespace RosettaUI.Builder
{
    public static class AnimationCurveHelper 
    {
        // AnimationCurve.CopyFrom()だと次のエラーになる場合があるので自前コピー
        // @Unity6000.0.55f1
        // > vector: assign data may not be data from the vector itself.
        // > UnityEngine.AnimationCurve:CopyFrom (UnityEngine.AnimationCurve)
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

        /// <summary>
        /// WrapModeも含めたHashCodeの取得
        /// 
        /// 通常のAnimationCurve.GetHashCode()はKeyframeのみ参照してWrapModeを含まない
        /// https://docs.unity3d.com/ScriptReference/AnimationCurve.GetHashCode.html
        /// </summary>
        public static int GetHashCodeWithWrapMode(this AnimationCurve curve)
        {
            return (curve, curve.preWrapMode, curve.postWrapMode).GetHashCode();
        }

        public static Rect GetCurveRect(this AnimationCurve curve, int stepNum = 64)
        {
            if (curve == null)
            {
                return default;
            }
            
            var keys = curve.keys;
            if (keys.Length <= 0)
            {
                return default;
            }

            var firstKey = keys[0];
            var rect = new Rect(firstKey.time, firstKey.value, 0f, 0f);
            
            for (var i = 1; i < keys.Length; i++)
            {
                var keyframe = keys[i];
                rect.xMin = Mathf.Min(rect.xMin, keyframe.time);
                rect.xMax = Mathf.Max(rect.xMax, keyframe.time);
                rect.yMin = Mathf.Min(rect.yMin, keyframe.value);
                rect.yMax = Mathf.Max(rect.yMax, keyframe.value);
            }
            
            for (var i = 0; i < stepNum; i++)
            {
                var y = curve.Evaluate(rect.xMin + i / (stepNum - 1f) * rect.width);
                rect.yMin = Mathf.Min(rect.yMin, y);
                rect.yMax = Mathf.Max(rect.yMax, y);
            }

            return rect;
        }
        
      
        /// <summary>
        /// インスペクターやUIToolkit標準のCurveFieldのようにカーブを表示する矩形の高さを再計算する
        /// 1.0未満で徐々にカーブの上下に余白を非線形に追加していく
        /// 正確な模倣ではない
        /// </summary>
        public static Rect AdjustCurveRectHeightLikeInspector(in Rect rect)
        {
            var height = rect.height;
            if (Mathf.Abs(height) >= 1f)
            {
                return rect;
            }
            
            const float powFactor = 10f;
            var padding = (Mathf.Pow(1f - height, powFactor)) * 0.5f;

            var ret = rect;
            ret.yMin -= padding;
            ret.yMax += padding;
            return ret;
        }
        
        /// <summary>
        /// カーブのプレビュー用のテクスチャを生成または更新する
        /// </summary>
        /// <returns>生成された場合はtrue、更新された場合はfalse</returns>
        public static bool GenerateOrUpdatePreviewTexture(
            AnimationCurve curve, 
            ref RenderTexture texture,
            int width, int height, 
            in AnimationCurvePreviewRenderer.CurvePreviewViewInfo viewInfo)
        {
            var textureGenerated = TextureUtility.EnsureTextureSize(ref texture, width, height);
            if (textureGenerated)
            {
                texture.name = "AnimationCurvePreview";
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
            }
            
            AnimationCurvePreviewRenderer.Render(curve, texture, viewInfo);
            
            // var rect = curve.GetCurveRect();
            // rect = AdjustCurveRectHeightLikeInspector(rect); 
            //
            //
            // AnimationCurvePreviewRenderer.Render(curve, texture, new AnimationCurvePreviewRenderer.CurvePreviewViewInfo
            // {
            //     offsetZoom = new Vector4(rect.min.x, rect.min.y, 1f / rect.width, 1f / rect.height),
            // });

            return textureGenerated;
        }


    }
}
