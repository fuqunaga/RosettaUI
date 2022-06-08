using System;

namespace RosettaUI
{
    public static partial class UI
    {
        /// <summary>
        /// Element to delay build call until Enable==true
        /// </summary>
        public static DynamicElement Lazy(Func<Element> build) => DynamicElementIf(() => true, build);
    }
}