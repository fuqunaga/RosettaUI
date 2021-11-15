namespace RosettaUI
{
    public class TextFieldElement : FieldBaseElement<string>
    {
        public bool MultiLine { get; set; }
        
        public TextFieldElement(LabelElement label, IBinder<string> binder) : base(label, binder) { }
    }
}