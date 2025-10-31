using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public readonly struct GridViewport
    {
        public float XOrder => Mathf.Log10(_width);
        public float YOrder => Mathf.Log10(_height);
        public float XTick => Mathf.Pow(10, Mathf.CeilToInt(XOrder) - 1);
        public float YTick => Mathf.Pow(10, Mathf.CeilToInt(YOrder) - 1);
        
        private readonly float _width;
        private readonly float _height;
        
        public GridViewport(float width, float height)
        {
            _width = width;
            _height = height;
        }

        public GridViewport(in Rect rect) : this(rect.width, rect.height) { }
        
        public float RoundX(float x, float tickMultiplier = 1)
        {
            var tick = XTick * tickMultiplier;
            return Mathf.Round(x / tick) * tick;
        }
        
        public float RoundY(float y, float tickMultiplier = 1)
        {
            var tick = YTick * tickMultiplier;
            return Mathf.Round(y / tick) * tick;
        }
    }
}