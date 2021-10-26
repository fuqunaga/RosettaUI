using UnityEngine;

namespace RosettaUI
{
    public struct Style
    {
        public enum Justify
        {
            Start,
            End
        }


        public float? width;
        public float? height;
        public float? minWidth;
        public float? minHeight;
        public Color? color;
        public Justify? justify;


        public bool HasValue => width.HasValue
                                || height.HasValue
                                || minWidth.HasValue || minHeight.HasValue
                                || color.HasValue
                                || justify.HasValue;
    }
}