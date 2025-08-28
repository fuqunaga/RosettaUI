using System.Linq;
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
                return Rect01();
            }
            
            var keys = curve.keys;
            if (keys.Length <= 0)
            {
                return Rect01();
            }
            
            var firstKeyPosition = keys[0].GetPosition();
            var xMin = firstKeyPosition.x;
            var xMax = keys.Last().time;
            var yMin = firstKeyPosition.y;
            var yMax = firstKeyPosition.y;

            for (var i = 0; i < keys.Length - 1; i++)
            {
                var (min, max) = CubicBezier.Create(keys[i], keys[i + 1]).CalcMinMaxY();
                yMin = Mathf.Min(yMin, min);
                yMax = Mathf.Max(yMax, max);
            }
            
            var rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

            
            if (Mathf.Approximately(rect.width, 0f))
            {
                rect.width = 1f;
                rect.x -= 0.5f;
            }
            
            if (Mathf.Approximately(rect.height, 0f))
            {
                rect.height = 1f;
                rect.y -= 0.5f;
            }

            return rect;
            
            static Rect Rect01()
            {
                return new Rect(0f, 0f, 1f, 1f);
            }
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

            return textureGenerated;
        }


    }
}
