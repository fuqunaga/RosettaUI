using System;

namespace RosettaUI
{
    public static partial class UI
    {
        public static LabelElement Label(LabelElement label) => label;
        public static LabelElement Label(Func<string> readLabel) => readLabel;
    }
}