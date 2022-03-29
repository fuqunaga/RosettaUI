using System;
using System.Collections;
using System.Linq;

namespace RosettaUI
{
    public class ListViewItemContainerElement : ElementGroup
    {
        private readonly IBinder _binder;
        public readonly ListViewOption option;
        
        private readonly Func<IBinder, int, Element> _createItemElement;
        
        
        public ListViewItemContainerElement(IBinder listBinder, Func<IBinder, int, Element> createItemElement, ListViewOption option) : base(null)
        {
            _binder = listBinder;
            _createItemElement = createItemElement;
            this.option = option;

            Interactable = !ListBinder.IsReadOnly(listBinder);
        }

        public IList GetIList() => ListBinder.GetIList(_binder);

        public void SetIList(IList iList) => _binder.SetObject(iList);

        
        public Element GetOrCreateItemElement(int index)
        {
            var element = GetContentAt(index);
            if (element == null)
            {
                var isReadOnly = ListBinder.IsReadOnly(_binder);
                for (var i = Contents.Count(); i <= index; i++)
                {
                    var itemBinder = ListBinder.CreateItemBinderAt(_binder, i);
                    element = _createItemElement(itemBinder, i);
                    if (!isReadOnly)
                    {
                        element = AddPopupMenu(element, _binder, i);
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
