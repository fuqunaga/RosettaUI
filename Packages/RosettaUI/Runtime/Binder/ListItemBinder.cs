using System.Collections.Generic;

namespace RosettaUI
{
    public class ListItemBinder<T> : ChildBinder<IList<T>, T>, IListItemBinder
    {
        private static IBinder<IList<T>> CreateIListBinder(IBinder binder)
        {
            return (IBinder<IList<T>>)CastBinder.Create(binder, binder.ValueType, typeof(IList<T>));
        }
        
        public int Index { get; set; }
        
        public ListItemBinder(IBinder listBinder, int index) : base(CreateIListBinder(listBinder))
        {
            Index = index;
        }

        protected override T GetFromParent(IList<T> list) => (0 <= Index && Index < list.Count) ? list[Index] : default;
        protected override IList<T> SetToParent(IList<T> list, T value)
        {
            if ( Index < list.Count ) list[Index] = value;
            return list; 
        }
    }
}