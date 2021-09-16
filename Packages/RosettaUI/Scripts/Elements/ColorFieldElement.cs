using UnityEngine;

namespace RosettaUI
{
    public class ColorFieldElement : FieldBaseElement<Color>
    {
        public ColorFieldElement(LabelElement label, BinderBase<Color> binder) : base(label, binder) { }
    }
}