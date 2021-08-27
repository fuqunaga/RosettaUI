namespace Comugi
{
    public static class MethodChainExtensions 
    {
        public static Element SetEnable(this Element e, bool enable)
        {
            e.enable = enable;
            return e;
        }

        public static Element SetInteractable(this Element e, bool interactable)
        {
            e.interactable = interactable;
            return e;
        }


        public static FoldElement Open(this FoldElement fold)
        {
            fold.isOpen = true;
            return fold;
        }
        public static FoldElement Close(this FoldElement fold)
        {
            fold.isOpen = false;
            return fold;
        }


        public static Element SetPreferredWidth(this Element element, int width)
        {
            var layout = element.layout;
            layout.preferredWidth = width;
            element.layout = layout;

            return element;
        }

        public static Element SetPreferredHeight(this Element element, int height)
        {
            var layout = element.layout;
            layout.preferredHeight = height;
            element.layout = layout;

            return element;
        }
    }
}