using RosettaUI.Reactive;

namespace RosettaUI
{
    public class FoldElement : ElementGroup
    {
        public readonly LabelElement title;
        public readonly Element contents;


        public readonly ReactiveProperty<bool> isOpenRx = new ReactiveProperty<bool>();
        protected bool _isOpen;

        public bool isOpen
        {
            get => isOpenRx.Value;
            set => isOpenRx.Value = value;
        }


        public FoldElement(LabelElement title, Element contents)
        {
            this.title = title;
            this.contents = contents;
            SetElements(new Element[] { this.title, this.contents });
        }
    }
}