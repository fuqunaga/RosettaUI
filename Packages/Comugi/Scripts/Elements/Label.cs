using System;

namespace Comugi
{
    public class Label : ReadOnlyValueElement<string>
    {
        public Label(IGetter<string> label) : base(label) { }


        public static implicit operator Label(string label) => new Label(ConstGetter.Create(label));
        public static implicit operator Label(Func<string> readLabel) => new Label(Getter.Create(readLabel));

        public static implicit operator string(Label label) => label.GetInitialValue();
    }
}