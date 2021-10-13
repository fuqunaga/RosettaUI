using UnityEngine;

namespace RosettaUI
{
    public class ColorFieldElement : FieldBaseElement<Color>
    {
        public ColorFieldElement(LabelElement label, IBinder<Color> binder) : base(label, binder) { }
    }
}