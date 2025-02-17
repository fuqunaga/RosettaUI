using UnityEngine;

namespace RosettaUI
{
    public class AnimationCurveElement : FieldBaseElement<AnimationCurve>
    {
        public AnimationCurveElement(LabelElement label, IBinder<AnimationCurve> binder) : base(label, binder) { }
    }
}