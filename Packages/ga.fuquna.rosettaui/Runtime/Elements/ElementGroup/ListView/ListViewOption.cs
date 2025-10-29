using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RosettaUI.UndoSystem;

namespace RosettaUI
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public struct ListViewOption
    {
        public static ListViewOption Default =>  new (true);
        
        
        public static ListViewOptionOfType<TItem> OfType<TItem>(IList<TItem> _)
        {
            return new ListViewOptionOfType<TItem>(Default);
        }
        
        
        public Func<IBinder, int, Element> createItemElementFunc;
        public Func<IList, Type, int, object> createItemInstanceFunc;
        public Func<object, IObjectRestoreRecord> createItemRestoreRecordFunc;
        
        public bool reorderable;
        public bool fixedSize;
        public bool header;
        public bool suppressAutoIndent;

        public ListViewOption(bool reorderable, bool fixedSize = false, bool header = true, bool suppressAutoIndent = false)
        {
            createItemElementFunc = null;
            createItemInstanceFunc = null;
            createItemRestoreRecordFunc = null;
            
            this.reorderable = reorderable;
            this.fixedSize = fixedSize;
            this.header = header;
            this.suppressAutoIndent = suppressAutoIndent;
        }

        public ListViewOption(ListViewOption other)
        {
            createItemElementFunc = other.createItemElementFunc;
            createItemInstanceFunc = other.createItemInstanceFunc;
            createItemRestoreRecordFunc = other.createItemRestoreRecordFunc;
            
            reorderable = other.reorderable;
            fixedSize = other.fixedSize;
            header = other.header;
            suppressAutoIndent = other.suppressAutoIndent;
        }
    }
    
    
    public static class ListViewOptionExtensions
    {
        public static ListViewOptionOfType<TItem> OfType<TItem>(this ListViewOption option, IList<TItem> _)
        {
            return new ListViewOptionOfType<TItem>(option);
        }
    }
    
    
    /// <summary>
    /// Wrapper class for ListViewOption specifying the item type of the list
    /// ListViewOption is type-independent, so use it to guarantee type safety
    /// </summary>
    public struct ListViewOptionOfType<TItem>
    {
        public static implicit operator ListViewOption (ListViewOptionOfType<TItem> optionOfType)
        {
            return optionOfType._option;
        }
        
        
        private ListViewOption _option;

        public ListViewOptionOfType(ListViewOption option)
        {
            _option = option;
        }
        
        public ListViewOptionOfType<TItem> SetCreateItemInstanceFunc(Func<IReadOnlyList<TItem>, int, TItem> func)
        {
            _option.createItemInstanceFunc = func == null 
                ? null 
                :  (list, _, index) => func((IReadOnlyList<TItem>)list, index);
            
            return this;
        }
        
        public ListViewOptionOfType<TItem> SetCreateItemRestoreRecordFunc(Func<TItem, IObjectRestoreRecord<TItem>> func)
        {
            _option.createItemRestoreRecordFunc = func == null
                ? null
                : obj => func((TItem)obj);
            
            return this;
        }

        public ListViewOptionOfType<TItem> SetItemRestoreFunc(Func<TItem, Func<TItem>> func)
        {
            return SetCreateItemRestoreRecordFunc(item => new ObjectRestoreRecord<TItem>(func(item)));
        }
    }
}