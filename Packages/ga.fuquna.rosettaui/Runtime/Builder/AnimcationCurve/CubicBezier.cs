using UnityEngine;

namespace RosettaUI.Builder
{
    /// <summary>
    /// Parameters for a cubic Bezier curve
    /// https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Cubic_B%C3%A9zier_curves
    /// </summary>
    public struct CubicBezier
    {
        public Vector2 p0; // start point
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3; // end point

        public static CubicBezier Create(Keyframe startKeyframe, Keyframe endKeyframe)
        {
            var key0 = startKeyframe;
            var key1 = endKeyframe;

            var deltaTime = key1.time - key0.time;

            var p0 = key0.GetPosition();
            var p3 = key1.GetPosition();
            var p1 = p0 + deltaTime * key0.GetVelocity(InOrOut.Out);
            var p2 = p3 - deltaTime * key1.GetVelocity(InOrOut.In);

            return new CubicBezier
            {
                p0 = p0,
                p1 = p1,
                p2 = p2,
                p3 = p3
            };
        }
    }

    public static class CubicBezierExtensions
    {
        // 3次ベジエ曲線のY最大値を求める
        // 曲線の方程式 ( B(t) = (1-t)^3P_0 + 3(1-t)^2tP_1 + 3(1-t)t^2P_2 + t^3P_3 ) のY成分について、
        // t（0～1）で微分し、極値（最大・最小）となるtを求めて、そのY値を計算します。
        public static (float, float) CalcMinMaxY(this CubicBezier bezier)
        {
            var p0 = bezier.p0;
            var p1 = bezier.p1;
            var p2 = bezier.p2;
            var p3 = bezier.p3;

            // 端点のY
            var minMaxY = p0.y < p3.y
                ? (min: p0.y, max: p3.y)
                : (min: p3.y, max: p0.y);
            
            // 制御点が無限大の場合は端点のYを返す
            // AnimationCurveの特殊処理
            if (float.IsInfinity(p1.y) || float.IsInfinity(p2.y))
            {
                return minMaxY;
            }
            
            // Y成分の係数
            // B(t) = a t^3 + b t^2 + c t + d
            var a = -p0.y + 3 * p1.y - 3 * p2.y + p3.y;
            var b = 3 * p0.y - 6 * p1.y + 3 * p2.y;
            var c = -3 * p0.y + 3 * p1.y;
            // var d = p0.y;

            
            // 微分して0になるtを求める（2次方程式）
            // 3at^2 + 2bt + c = 0
            var discriminant = b * b - 3 * a * c;
            if (!Mathf.Approximately(a, 0) && discriminant >= 0)
            {
                var sqrtD = Mathf.Sqrt(discriminant);
                var t1 = (-b + sqrtD) / (3 * a);
                var t2 = (-b - sqrtD) / (3 * a);
                
                for (var i = 0; i < 2; ++i)
                {
                    var t = i == 0 ? t1 : t2;
                    var y = CubicBezierY(p0.y, p1.y, p2.y, p3.y, t);
                    minMaxY.min = Mathf.Min(minMaxY.min, y);
                    minMaxY.max = Mathf.Max(minMaxY.max, y);
                }
            }

            return minMaxY;
        }

        // 3次ベジエ曲線のY値
        private static float CubicBezierY(float y0, float y1, float y2, float y3, float t)
        {
            var u = 1 - t;
            return u * u * u * y0
                   + 3 * u * u * t * y1
                   + 3 * u * t * t * y2
                   + t * t * t * y3;
        }
    }
}