using System;
using UnityEngine;

namespace RosettaUI
{
    public class Style : IEquatable<Style>
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
        public float? maxWidth;
        public float? maxHeight;
        public Color? color;
        public Justify? justify;


        public bool HasValue => width.HasValue
                                || height.HasValue
                                || minWidth.HasValue || minHeight.HasValue
                                || maxWidth.HasValue || maxHeight.HasValue
                                || color.HasValue
                                || justify.HasValue;

        public bool Equals(Style other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(width, other.width) 
                   && Nullable.Equals(height, other.height)
                   && Nullable.Equals(minWidth, other.minWidth)
                   && Nullable.Equals(minHeight, other.minHeight) 
                   && Nullable.Equals(color, other.color)
                   && justify == other.justify;
        }
    }
}