using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RosettaUI
{
    public static partial class UI
    {
        private static readonly Dictionary<Type, MethodInfo> ClipboardContextMenuMethodDictionary = new();

        public static PopupMenuElement ClipboardContextMenu(Element element, IBinder binder)
        {
            var valueType = binder.ValueType;
            if (!ClipboardContextMenuMethodDictionary.TryGetValue(valueType, out var mi))
            {
                mi = typeof(UI).GetMethods()
                    .First(m => m.Name == nameof(ClipboardContextMenu) && m.IsGenericMethod)
                    .MakeGenericMethod(valueType);
                    
                ClipboardContextMenuMethodDictionary[valueType] = mi;
            }
            
            return (PopupMenuElement) mi.Invoke(null, new object[] { element, binder });
        }
        
        public static PopupMenuElement ClipboardContextMenu<T>(Element element, IBinder<T> binder)
        {
            return Popup(element, ClipboardUtility.GenerateContextMenuItemsFunc(
                binder.Get,
                value =>
                {
                    binder.Set(value);
                    element.NotifyViewValueChanged();
                },
                element)
            );
        }
    }
}