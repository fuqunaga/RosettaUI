namespace RosettaUI
{
    public class ToggleElement : FieldBaseElement<bool>
    {
        public bool isLabelRight;

        public ToggleElement(LabelElement label, IBinder<bool> binder, bool isLabelRight = false) 
            : base(label, binder)
        {
            this.isLabelRight = isLabelRight;
        }
    }
}