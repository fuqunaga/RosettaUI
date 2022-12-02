namespace RosettaUI
{
    public class TextFieldElement : FieldBaseElement<string>
    {
        public bool IsMultiLine { get; internal set; }
        
        public TextFieldElement(LabelElement label, IBinder<string> binder) : base(label, binder) { }
    }
}