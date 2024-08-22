namespace RosettaUI
{
    public class HelpBoxElement : Element
    {
        public readonly LabelElement label;
        public readonly HelpBoxType helpBoxType;

        public HelpBoxElement(LabelElement label, HelpBoxType helpBoxType = HelpBoxType.None)
        {
            this.label = label;
            this.helpBoxType = helpBoxType;
        }

        protected override void UpdateInternal()
        {
            base.UpdateInternal();
            label.Update();
        }
    }
}