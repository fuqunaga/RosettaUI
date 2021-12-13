using System;
using System.Collections.Generic;

namespace RosettaUI.UIToolkit
{
    public class PopupField<T> : UnityInternalAccess.PopupField<T>
    {
        public PopupField(List<T> choices, T defaultValue, Func<T, string> formatSelectedValueCallback = null, Func<T, string> formatListItemCallback = null)
            : base(choices, defaultValue, formatSelectedValueCallback, formatListItemCallback)
        {
        }

        public PopupField(string label, List<T> choices, T defaultValue, Func<T, string> formatSelectedValueCallback = null, Func<T, string> formatListItemCallback = null)
            : base(label, choices, defaultValue, formatSelectedValueCallback, formatListItemCallback)
        {
        }

        public PopupField(List<T> choices, int defaultIndex, Func<T, string> formatSelectedValueCallback = null, Func<T, string> formatListItemCallback = null)
            : base(choices, defaultIndex, formatSelectedValueCallback, formatListItemCallback)
        {
        }

        public PopupField(string label, List<T> choices, int defaultIndex, Func<T, string> formatSelectedValueCallback = null, Func<T, string> formatListItemCallback = null)
            : base( label, choices,  defaultIndex, formatSelectedValueCallback ,  formatListItemCallback)
        {
        }
    }
}