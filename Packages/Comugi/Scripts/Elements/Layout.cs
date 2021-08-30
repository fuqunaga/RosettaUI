namespace RosettaUI
{
    public struct Layout
    {
        public int? preferredWidth;
        public int? preferredHeight;

        public bool HasValue => preferredWidth.HasValue ||  preferredHeight.HasValue;
    }
}