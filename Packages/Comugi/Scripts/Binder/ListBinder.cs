using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static class ListBinder
    {
        public static IEnumerable<BinderBase<T>> CreateItemBindersFrom<T>(IList<T> list)
        {
            return Enumerable.Range(0, list.Count).Select(i => new ListItemBinder<T>(ConstGetter.Create(list), i));
        }

        public static IEnumerable<IBinder> CreateItemBindersFrom(IGetter<IList> listGetter)
        {
            var type = listGetter.ValueType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));

            Assert.IsNotNull(type, $"{type} does not Inherit from IList<>.");
            var itemType = type.GetGenericArguments()[0];

            var binderType = typeof(ListItemBinder<>).MakeGenericType(itemType);


            return Enumerable.Range(0, listGetter.Get().Count).Select(i =>
            {
                return Activator.CreateInstance(binderType, listGetter, i) as IBinder;
            });
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