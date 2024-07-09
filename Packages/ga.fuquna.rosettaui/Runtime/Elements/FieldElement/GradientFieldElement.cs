using UnityEngine;

namespace RosettaUI
{
    public class GradientFieldElement : FieldBaseElement<Gradient>
    {
        public GradientFieldElement(LabelElement label, IBinder<Gradient> binder) : base(label, binder) { }
    }
}