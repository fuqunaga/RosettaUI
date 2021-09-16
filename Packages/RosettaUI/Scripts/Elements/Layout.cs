namespace RosettaUI
{
    public struct Layout
    {
        public enum Justify
        {
            Start,
            End
        }

        public int? width;
        public int? height;
        public Justify? justify;

        public bool HasValue => width.HasValue ||  height.HasValue || justify.HasValue;
    }
}