using UnityEngine;

namespace RosettaUI
{
    public class ColorFieldElement : FieldBaseElement<Color>
    {
        protected override bool ShouldRecordUndo => false;
        
        public ColorFieldElement(LabelElement label, IBinder<Color> binder) : base(label, binder) { }
    }
}