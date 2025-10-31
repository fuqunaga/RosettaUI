using System;
using System.Collections;
using UnityEngine.Assertions;

namespace RosettaUI
{
    using CreateItemInstanceFunc = Func<IList, Type, int, object>;

    public readonly partial struct ListBinder
    {
        private readonly IBinder _binder;
        private readonly CreateItemInstanceFunc _createItemInstanceFunc;

        public Type ListType => _binder.ValueType;
        public Type ItemType => ListUtility.GetItemType(ListType);
        
        public ListBinder(IBinder binder, CreateItemInstanceFunc createItemInstanceFunc)
        {
            Assert.IsTrue(IsListBinder(binder));

            _binder = binder;
            _createItemInstanceFunc = createItemInstanceFunc ?? DuplicateItemInstance;

            return;

            
            static object DuplicateItemInstance(IList list, Type itemType, int index)
            {
                var previousIndex = index - 1;
                var baseItem = (previousIndex < 0 || previousIndex >= list.Count)
                    ? null
                    : list[previousIndex];
                
                return ListUtility.CreateNewItem(baseItem, itemType);
            }
        }

        public IListItemBinder CreateItemBinderAt(int index) => CreateItemBinderAt(_binder, index);
        public IList GetIList() => GetIList(_binder);

        public int GetCount() => GetCount(_binder);

        public void SetCount(int count)
        {
            var current = GetCount();
            var diff = count - current;
            for (var i = 0; i < diff; ++i)
            {
                AddItemAtLast();
            }

            for (var i = 0; i < -diff; ++i)
            {
                RemoveItemAtLast();
            }
        }

        public bool IsReadOnly() => IsReadOnly(_binder);

        public void AddItem(int index)
        {
            var newItem = CreateNewItem(index);
            AddItem(newItem, index);
        }
        
        public void AddItem(object newItem, int index)
        {
            var list = GetIList();
            list = ListUtility.AddItem(list, ItemType, newItem, index);
            _binder.SetObject(list);
        }

        public void AddNullItem(int index) => AddNullItem(_binder, index);
        public void RemoveItem(int index) => RemoveItem(_binder, index);
        public void RemoveItems(Range range) => RemoveItems(_binder, range);
        public void MoveItem(int fromIndex, int toIndex) => MoveItem(_binder, fromIndex, toIndex);

        public void AddItemAtLast()
        {
            var list = GetIList() ?? (IList) Activator.CreateInstance(ListType);
            
            var index = list?.Count ?? 0;
            AddItem(index);
        }
        
        public void RemoveItemAtLast() => RemoveItemAtLast(_binder);
        
        
        private object CreateNewItem(int index)
        {
            var list = GetIList();
            return _createItemInstanceFunc?.Invoke(list, ItemType, index);
        }
    }
}