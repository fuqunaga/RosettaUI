namespace RosettaUI
{
    public class IntFieldElement : FieldBaseElement<int>
    {
        public readonly bool isUnsigned;

        public IntFieldElement(LabelElement label, IBinder<int> binder, bool unsigned = false) : base(label, binder) => this.isUnsigned = unsigned;
    }
}