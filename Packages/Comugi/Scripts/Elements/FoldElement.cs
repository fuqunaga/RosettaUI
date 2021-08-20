namespace Comugi
{
    public class FoldElement : ElementGroup
    {
        public readonly Label title;
        public readonly Element contents;


        protected bool _isOpen;

        public bool isOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    contents.enable = _isOpen;
                    ViewBridge.SetFoldOpen(this, _isOpen);
                }
            }
        }


        public FoldElement(Label title, Element contents)
        {
            this.title = title;
            this.contents = contents;
            SetElements(new Element[] { this.title, this.contents });
        }
    }
}