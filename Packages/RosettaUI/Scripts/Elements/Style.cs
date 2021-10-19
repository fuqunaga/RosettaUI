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

        public Color? color;
        public int? minWidth;
        public int? minHeight;
        public Justify? justify;
        

        public bool HasValue => color.HasValue || minWidth.HasValue ||  minHeight.HasValue || justify.HasValue;
    }
}