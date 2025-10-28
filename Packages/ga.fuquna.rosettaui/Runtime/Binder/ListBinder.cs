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
            _createItemInstanceFunc = createItemInstanceFunc;
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

        public void DuplicateItem(int index) => DuplicateItem(_binder, index);
        public void AddItem(int index) => AddItem(_binder, index);
        public void AddNullItem(int index) => AddNullItem(_binder, index);
        public void RemoveItem(int index) => RemoveItem(_binder, index);
        public void RemoveItems(Range range) => RemoveItems(_binder, range);
        public void MoveItem(int fromIndex, int toIndex) => MoveItem(_binder, fromIndex, toIndex);

        public void AddItemAtLast()
        {
            var list = GetIList() ?? (IList) Activator.CreateInstance(ListType);
            
            var index = list?.Count ?? 0;
            var newItem = CreateNewItem(list, index);

            list = ListUtility.AddItem(list, ItemType, newItem, index);
            
            _binder.SetObject(list);
        }
        
        public void RemoveItemAtLast() => RemoveItemAtLast(_binder);
        
        
        private object CreateNewItem(IList list, int index)
        {
            if (_createItemInstanceFunc != null)
            {
                return _createItemInstanceFunc(list, ItemType, index);
            }
            
            var baseIndex = Math.Max(0, list.Count - 1);
            var baseItem = list.Count > 0 ? list[baseIndex] : null;
            return ListUtility.CreateNewItem(baseItem, ItemType);
        }
    }
}