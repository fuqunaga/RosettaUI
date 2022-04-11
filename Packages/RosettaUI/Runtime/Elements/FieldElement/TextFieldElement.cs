namespace RosettaUI
{
    public class TextFieldElement : FieldBaseElement<string>
    {
        public bool IsMultiLine { get; set; }
        
        public TextFieldElement(LabelElement label, IBinder<string> binder) : base(label, binder) { }
    }
}