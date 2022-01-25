using System;

namespace RosettaUI
{
    public static partial class UI
    {
        public static LabelElement Label(LabelElement label, bool isPrefix = false)
        {
            label.isPrefix = isPrefix;
            return label;
        }

        public static LabelElement Label(Func<string> readLabel, bool isPrefix = false) =>
            Label((LabelElement) readLabel, isPrefix);
    }
}