using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static class ListBinder
    {
        public static IEnumerable<BinderBase<T>> CreateItemBinders<T>(IList<T> list)
        {
            return Enumerable.Range(0, list.Count).Select(i => new ListItemBinder<T>(ConstGetter.Create(list), i));
        }

        
        
        
        public static IEnumerable<IBinder> CreateItemBinders(IBinder listBinder)
        {
            static bool IsIList(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>);
            
            var type = listBinder.ValueType;
            var listType = IsIList(type)
                ? type
                : type.GetInterfaces().FirstOrDefault(IsIList);
            Assert.IsNotNull(listType, $"{type} does not Inherit from IList<>.");
            
            var itemType = listType.GetGenericArguments()[0];
            var binderType = typeof(ListItemBinder<>).MakeGenericType(itemType);

            var itemCount = GetCount(listBinder);
            return Enumerable.Range(0, itemCount)
                .Select(i => Activator.CreateInstance(binderType, listBinder, i) as IBinder);
        }

        public static IList GetIList(IBinder binder) => binder.GetObject() as IList;
        
        public static int GetCount(IBinder binder)
        {
            return GetIList(binder)?.Count ?? 0;
        }

        public static void SetCount(IBinder binder, int count)
        {
            var current = GetCount(binder);
            var diff = count - current;
            for (var i = 0; i < diff; ++i)
            {
                AddItemAtLast(binder);
            }

            for (var i = 0; i < -diff; ++i)
            {
                RemoveItemAtLast(binder);
            }
        }

        public static bool IsListBinder(IBinder binder)
        {
            var type = binder.ValueType;
            return typeof(IList).IsAssignableFrom(type)
                && type.GetInterfaces()
                    .Where(t => t.IsGenericType)
                    .Select(t => t.GetGenericTypeDefinition())
                    .Contains(typeof(IList<>));
        }

        public static bool IsReadOnly(IBinder binder) => binder.IsReadOnly || (GetIList(binder)?.IsReadOnly ?? false);


        public static void DuplicateItem(IBinder binder, int index)
        {
            var list = GetIList(binder);
            
            var itemType = TypeUtility.GetListItemType(binder.ValueType);
            
            list = ListUtility.AddItem(list, itemType, list[index], index);
            binder.SetObject(list);
        }

        public static void RemoveItem(IBinder binder, int index)
        {
            var list = GetIList(binder);
            
            var itemType = TypeUtility.GetListItemType(binder.ValueType);
            
            list = ListUtility.RemoveItem(list, itemType, index);
            binder.SetObject(list);
        }

        public static void AddItemAtLast(IBinder binder)
        {
            var list = GetIList(binder);
            
            var listType = binder.ValueType;
            var itemType = TypeUtility.GetListItemType(binder.ValueType);


            list = ListUtility.AddItemAtLast(list, listType, itemType);
            binder.SetObject(list);
        }

        public static void RemoveItemAtLast(IBinder binder)
        {
            var list = GetIList(binder);
            
            var itemType = TypeUtility.GetListItemType(binder.ValueType);
            list = ListUtility.RemoveItemAtLast(list, itemType);
            binder.SetObject(list);
        }
    }
    

    public class ListItemBinder<T> : ChildBinder<IList<T>, T>
    {
        public ListItemBinder(IGetter<IList<T>> listGetter, int idx) : 
            base(new Binder<IList<T>>(listGetter, null),
                (list) => list[idx],
                (list, v) => {
                    if ( idx < list.Count ) list[idx] = v;
                    return list; 
                }
                )
        {
        }
    }
}