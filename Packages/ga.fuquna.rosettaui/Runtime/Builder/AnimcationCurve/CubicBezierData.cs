using UnityEngine;

namespace RosettaUI.Builder
{
    /// <summary>
    /// Parameters for a cubic Bezier curve
    /// https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Cubic_B%C3%A9zier_curves
    /// </summary>
    public struct CubicBezierData
    {
        public Vector2 p0; // start point
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3; // end point
    }
}