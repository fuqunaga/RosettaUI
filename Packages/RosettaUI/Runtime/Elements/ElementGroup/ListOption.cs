namespace RosettaUI
{
    public struct ListOption
    {
        public static ListOption Default =>  new (true);
        
        public bool reorderable;
        public bool fixedSize;
        public bool header;

        public ListOption(bool reorderable, bool fixedSize = false, bool header = true)
        {
            this.reorderable = reorderable;
            this.fixedSize = fixedSize;
            this.header = header;
        }
    }
}