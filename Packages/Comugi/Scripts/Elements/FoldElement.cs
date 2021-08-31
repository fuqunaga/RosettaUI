using RosettaUI.Reactive;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public class FoldElement : ElementGroup
    {
        public readonly LabelElement title;
        protected readonly List<Element> contents;
        public IReadOnlyCollection<Element> Contents => contents;


        public readonly ReactiveProperty<bool> isOpenRx = new ReactiveProperty<bool>();
        protected bool _isOpen;

        public bool isOpen
        {
            get => isOpenRx.Value;
            set => isOpenRx.Value = value;
        }


        public FoldElement(LabelElement title, IEnumerable<Element> contents)
        {
            this.title = title;
            this.contents = contents.Where(e => e != null).ToList();
            SetElements(new Element[] { this.title }.Concat(Contents));
        }
    }
}