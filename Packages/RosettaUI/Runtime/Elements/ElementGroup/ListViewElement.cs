using System;
using System.Collections;
using System.Linq;

namespace RosettaUI
{
    public class ListViewElement : ElementGroupWithHeader
    {
        private readonly IBinder binder;
        public readonly ListViewOption option;
        
        private readonly Func<IBinder, int, Element> _createItemElement;
        public LabelElement Label => (LabelElement)header;
        
        public ListViewElement(LabelElement label, IBinder listBinder, Func<IBinder, int, Element> createItemElement, ListViewOption option) : base(label, null)
        {
            binder = listBinder;
            _createItemElement = createItemElement;
            this.option = option ?? ListViewOption.Default;

            Interactable = !listBinder.IsReadOnly && !GetIList().IsReadOnly;
        }

        public IList GetIList() => ListBinder.GetIList(binder);

        public void SetIList(IList iList) => binder.SetObject(iList);

        
        public Element GetOrCreateItemElement(int index)
        {
            var element = GetContentAt(index);
            if (element == null)
            {
                var isReadOnly = ListBinder.IsReadOnly(binder);
                
                for(var i=Contents.Count(); i<=index; i++)
                {
                    var itemBinder = ListBinder.CreateItemBinderAt(binder, i);
                    element = _createItemElement(itemBinder, i);
                    if (!isReadOnly)
                    {
                        element = AddPopupMenu(element, binder, i);
                    }
                    
                    AddChild(element);
                }
            }

            return element;
        }

        public static Element AddPopupMenu(Element element, IBinder binder, int idx)
        {
            return new PopupMenuElement(
                element,
                () => new[]
                {
                    new MenuItem("Add Element", () => ListBinder.DuplicateItem(binder, idx)),
                    new MenuItem("Remove Element", () => ListBinder.RemoveItem(binder, idx)),
                }
            );
        }
    }

    public class ListViewOption
    {
        private static ListViewOption _default;
        public static ListViewOption Default => _default ?? new (true, false);
        
        public readonly bool reorderable;
        public readonly bool fixedSize;

        public ListViewOption(bool reorderable, bool fixedSize)
        {
            this.reorderable = reorderable;
            this.fixedSize = fixedSize;
        }
    }
}