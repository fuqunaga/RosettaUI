// ReSharper disable FieldCanBeMadeReadOnly.Global
namespace RosettaUI
{
    public struct ListViewOption
    {
        public static ListViewOption Default =>  new (true);
        
        public bool reorderable;
        public bool fixedSize;
        public bool header;
        public bool suppressAutoIndent;

        public ListViewOption(bool reorderable, bool fixedSize = false, bool header = true, bool suppressAutoIndent = false)
        {
            this.reorderable = reorderable;
            this.fixedSize = fixedSize;
            this.header = header;
            this.suppressAutoIndent = suppressAutoIndent;
        }
    }
}