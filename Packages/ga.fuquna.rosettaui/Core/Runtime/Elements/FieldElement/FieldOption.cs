namespace RosettaUI
{
    public struct FieldOption
    {
        public static FieldOption Default => new();
        public bool delayInput;
        public bool suppressClipboardContextMenu;
    }
}