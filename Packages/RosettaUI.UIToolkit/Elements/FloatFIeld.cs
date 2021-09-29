using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
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
}