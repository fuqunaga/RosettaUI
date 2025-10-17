using UnityEngine;

namespace RosettaUI
{
    public class GradientFieldElement : FieldBaseElement<Gradient>
    {
        protected override bool ShouldRecordUndo => false;
        
        public GradientFieldElement(LabelElement label, IBinder<Gradient> binder) : base(label, binder) { }
    }
}