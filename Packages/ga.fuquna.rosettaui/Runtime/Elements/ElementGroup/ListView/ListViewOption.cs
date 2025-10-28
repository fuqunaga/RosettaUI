using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Assertions;

namespace RosettaUI
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public struct ListViewOption
    {
        public static ListViewOption Default =>  new (true);
        
        
        public static ListViewOptionOfType<TList> OfType<TList>(TList _)
            where TList : IList
        {
            return new ListViewOptionOfType<TList>(Default);
        }
        
        
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
    
    public static class ListViewOptionExtensions
    {
        public static ListViewOptionOfType<TList> OfType<TList>(this ListViewOption option, TList _)
            where TList : IList
        {
            return new ListViewOptionOfType<TList>(option);
        }
    }

    /// <summary>
    /// ListViewOption specialized for List types.
    /// Acts as a wrapper to enable type inference and omit type arguments when using SetCreateItemInstanceFunc().
    /// </summary>
    public struct ListViewOptionOfType<TList>
        where TList : IList
    {
        public static implicit operator ListViewOption (ListViewOptionOfType<TList> optionOfType)
        {
            return optionOfType._option;
        }
        
        private ListViewOption _option;

        public ListViewOptionOfType(ListViewOption option)
        {
            _option = option;
        }
        
        public ListViewOption SetCreateItemInstanceFunc<TItem>(Func<TList, int, TItem> func)
        {
            Assert.IsTrue(ListUtility.GetItemType(typeof(TList)).IsAssignableFrom(typeof(TItem)),
                $"TItem ({typeof(TItem)}) must be assignable to the item type of TList ({ListUtility.GetItemType(typeof(TList))})"
            );
            
            _option.createItemInstanceFunc = (list, _, index) => func((TList)list, index);
            return _option;
        }
    }
}