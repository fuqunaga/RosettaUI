#define AvoidInternal

namespace RosettaUI.UIToolkit
{
    public class PopupField<T> : PackageInternal.PopupField<T>
    {
#if !AvoidInternal
        public new class UxmlFactory : UxmlFactory<PopupField<T>, UxmlTraits>
        {
        }
#endif

        public PopupField() : base()
        {
        }

#if !AvoidInternal
        public PopupField(string label = null) : base(label)
        {
        }

        
        public PopupField(List<T> choices, T defaultValue, Func<T, string> formatSelectedValueCallback =
 null, Func<T, string> formatListItemCallback = null)
            : base(choices, defaultValue, formatSelectedValueCallback, formatListItemCallback)
        {
        }

        public PopupField(string label, List<T> choices, T defaultValue, Func<T, string> formatSelectedValueCallback =
 null, Func<T, string> formatListItemCallback = null)
            : base(label, choices, defaultValue, formatSelectedValueCallback, formatListItemCallback)
        {
        }

        public PopupField(List<T> choices, int defaultIndex, Func<T, string> formatSelectedValueCallback =
 null, Func<T, string> formatListItemCallback = null)
            : base(choices, defaultIndex, formatSelectedValueCallback, formatListItemCallback)
        {
        }

        public PopupField(string label, List<T> choices, int defaultIndex, Func<T, string> formatSelectedValueCallback =
 null, Func<T, string> formatListItemCallback = null)
            : base( label, choices,  defaultIndex, formatSelectedValueCallback ,  formatListItemCallback)
        {
        }
#endif
    }
}