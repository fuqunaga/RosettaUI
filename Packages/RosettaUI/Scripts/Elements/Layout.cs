namespace RosettaUI
{
    public struct Layout
    {
        public enum Justify
        {
            Start,
            End
        }

        public int? minWidth;
        public int? minHeight;
        public Justify? justify;

        public bool HasValue => minWidth.HasValue ||  minHeight.HasValue || justify.HasValue;
    }
}