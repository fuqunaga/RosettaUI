using RosettaUI.Reactive;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public abstract class OpenCloseBaseElement : ElementGroup
    {
        public readonly LabelElement title;
        protected readonly List<Element> contents;
        public override IEnumerable<Element> Contents => contents;


        public readonly ReactiveProperty<bool> isOpenRx = new ReactiveProperty<bool>();
        protected bool isOpen;

        public bool IsOpen
        {
            get => isOpenRx.Value;
            set => isOpenRx.Value = value;
        }


        public OpenCloseBaseElement(LabelElement title, IEnumerable<Element> contents)
        {
            this.title = title;
            this.contents = contents.Where(e => e != null).ToList();
            SetElements(new Element[] { this.title }.Concat(Contents));
        }
    }
}