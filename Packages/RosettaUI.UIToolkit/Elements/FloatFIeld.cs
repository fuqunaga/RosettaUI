//#define AvoidInternal

using RosettaUI.UIToolkit;
using UnityEngine.UIElements;
using FloatField = UnityEditor.UIElements.FloatField;

namespace RosettaUI.UIToolkit
{
    
#if AvoidInternal
    public class FloatField : TextInputBaseField<float>
    {

        public FloatField() : this(null, -1, char.MinValue, null)
        {
        }

        protected FloatField(string label, int maxLength, char maskChar, TextInputBase textInputBase) : base(label, maxLength, maskChar, textInputBase)
        {
        }
    }
#else
    public class FloatField : PackageInternal.FloatField
    {
        public new class UxmlFactory : UxmlFactory<FloatField, UxmlTraits>
        {
        }

        public FloatField() : base()
        {
        }


        public FloatField(int maxLength) : base(null, maxLength)
        {
        }

        public FloatField(string label, int maxLength) : base(label, maxLength)
        {
        }

        public FloatField(string label) : base(label)
        {
        }
    }
#endif
}