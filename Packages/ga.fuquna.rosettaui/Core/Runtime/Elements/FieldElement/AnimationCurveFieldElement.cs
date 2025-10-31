using UnityEngine;

namespace RosettaUI
{
    public class AnimationCurveElement : FieldBaseElement<AnimationCurve>
    {
        protected override bool ShouldRecordUndo => false;
        
        public AnimationCurveElement(LabelElement label, IBinder<AnimationCurve> binder) : base(label, binder) { }
    }
}