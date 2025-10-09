using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RosettaUI
{
    public static class ListUtility
    {
        private static readonly Dictionary<Type, Type> ListItemTypeTable = new();
        
        public static Type GetItemType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            
            if (!ListItemTypeTable.TryGetValue(type, out var itemType))
            {
                itemType = type.GetInterfaces().Concat(new[] {type})
                    .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>))
                    .Select(t => t.GetGenericArguments().First())
                    .FirstOrDefault();

                ListItemTypeTable[type] = itemType;
            }

            return itemType;
        } 
        
        public static IList AddItemAtLast(IList list, Type type, Type itemType)
        {
            list ??= (IList) Activator.CreateInstance(type);

            var baseItem = list.Count > 0 ? list[^1] : null;

            return AddItem(list, itemType, baseItem, list.Count);
        }

        public static IList RemoveItemAtLast(IList target, Type itemType)
        {
            return RemoveItem(target, itemType, target.Count - 1);
        }

        public static IList AddItem(IList list, Type elemType, object baseItem, int index)
        {
            var newElem = CreateNewItem(baseItem, elemType);
            return DoAddItem(list, elemType, newElem, index);
        }
        
        public static IList AddNullItem(IList list, Type elemType, int index)
        {
            return DoAddItem(list, elemType, null, index);
        }
        
        private static IList DoAddItem(IList list, Type elemType, object newItem, int index)
        {
            index = Mathf.Clamp(index, 0, list.Count);

            if (list is Array array)
            {
                var newArray = Array.CreateInstance(elemType, array.Length + 1);
                Array.Copy(array, newArray, index);
                newArray.SetValue(newItem, index);
                Array.Copy(array, index, newArray, index + 1, array.Length - index);
                list = newArray;
            }
            else
            {
                list.Insert(index, newItem);
            }

            return list;
        }

        public static IList RemoveItem(IList list, Type itemType, int index)
        {
            if (list is Array array)
            {
                var newArray = Array.CreateInstance(itemType, array.Length - 1);
                Array.Copy(array, newArray, index);
                Array.Copy(array, index + 1, newArray, index, array.Length - 1 - index);
                list = newArray;
            }
            else
            {
                list.RemoveAt(index);
            }

            return list;
        }


        private static object CreateNewItem(object baseItem, Type itemType)
        {
            object ret = null;

            if (baseItem != null)
            {
                // is cloneable
                if (baseItem is ICloneable cloneable)
                {
                    ret = cloneable.Clone();
                }
                else if (itemType.IsValueType)
                {
                    ret = baseItem;
                }
                // has copy constructor
                else if (itemType.GetConstructor(new[] { itemType }) != null)
                {
                    ret = Activator.CreateInstance(itemType, baseItem);
                }
            }

            ret ??= (itemType == typeof(string))
                ? ""
                : Activator.CreateInstance(itemType);

            return ret;
        }
    }
}