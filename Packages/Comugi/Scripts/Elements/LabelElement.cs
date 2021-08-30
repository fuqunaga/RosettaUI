using System;

namespace RosettaUI
{
    public class LabelElement : ReadOnlyValueElement<string>
    {
        public LabelElement(IGetter<string> label) : base(label) { }


        public static implicit operator LabelElement(string label) => new LabelElement(ConstGetter.Create(label));
        public static implicit operator LabelElement(Func<string> readLabel) => new LabelElement(Getter.Create(readLabel));

        public static implicit operator string(LabelElement label) => label.GetInitialValue();
    }
}