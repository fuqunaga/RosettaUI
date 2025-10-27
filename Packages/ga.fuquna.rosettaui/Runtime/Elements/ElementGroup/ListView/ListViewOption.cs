using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace RosettaUI
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public struct ListViewOption
    {
        public static ListViewOption Default =>  new (true);
        
        public Func<IBinder, int, Element> createItemElementFunc;
        public Func<IList, Type, int, object> createItemInstanceFunc;
        
        public bool reorderable;
        public bool fixedSize;
        public bool header;
        public bool suppressAutoIndent;

        public ListViewOption(bool reorderable, bool fixedSize = false, bool header = true, bool suppressAutoIndent = false)
        {
            createItemElementFunc = null;
            createItemInstanceFunc = null;
            
            this.reorderable = reorderable;
            this.fixedSize = fixedSize;
            this.header = header;
            this.suppressAutoIndent = suppressAutoIndent;
        }

        public ListViewOption(ListViewOption other)
        {
            createItemElementFunc = other.createItemElementFunc;
            createItemInstanceFunc = other.createItemInstanceFunc;
            
            reorderable = other.reorderable;
            fixedSize = other.fixedSize;
            header = other.header;
            suppressAutoIndent = other.suppressAutoIndent;
        }
    }
}