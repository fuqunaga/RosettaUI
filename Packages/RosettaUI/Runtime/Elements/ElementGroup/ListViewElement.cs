using System;
using System.Collections;
using System.Linq;

namespace RosettaUI
{
    public class ListViewElement : ElementGroupWithHeader
    {
        public readonly IBinder binder;
        private readonly Func<IBinder, int, Element> _createItemElement;
        public LabelElement Label => (LabelElement)header;
        
        public ListViewElement(LabelElement label, IBinder listBinder, Func<IBinder, int, Element> createItemElement) : base(label, null)
        {
            binder = listBinder;
            _createItemElement = createItemElement;
        }

        protected override void UpdateInternal()
        {
            var count = ListBinder.GetCount(binder);
            while(Contents.Count() > count)
            {
                Contents.Last().Destroy();
            }

            while(Contents.Count() < count)
            {
                AddItemElementAtLast();
            }

            base.UpdateInternal();
        }

        public IList GetIList() => ListBinder.GetIList(binder);

        public void SetIList(IList iList) => binder.SetObject(iList);

        public int GetListItemCount() => ListBinder.GetCount(binder);

        
        public Element GetOrCreateItemElement(int index)
        {
            var element = GetContentAt(index);
            if (element == null)
            {
                for(var i=Contents.Count(); i<=index; i++)
                {
                    var itemBinder = ListBinder.CreateItemBinderAt(binder, i);
                    element = _createItemElement(itemBinder, i);
                    AddChild(element);
                }
            }

            return element;
        }

        void AddItemElementAtLast()
        {
            var i = Contents.Count();
            var itemBinder = ListBinder.CreateItemBinderAt(binder, i);
            var element = _createItemElement(itemBinder, i);
            AddChild(element);
        }
    }
}