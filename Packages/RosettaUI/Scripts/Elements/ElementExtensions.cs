namespace RosettaUI
{
    public static class ElementExtensions_MethodChain
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


        public static Element SetWidth(this Element element, int width)
        {
            var layout = element.layout;
            layout.width = width;
            element.layout = layout;

            return element;
        }

        public static Element SetHeight(this Element element, int height)
        {
            var layout = element.layout;
            layout.height = height;
            element.layout = layout;

            return element;
        }

        public static Element SetJustify(this Element element, Layout.Justify justify)
        {
            var layout = element.layout;
            layout.justify = justify;
            element.layout = layout;
            return element;
        }

        public static Element SetLayout(this Element element, Layout layout)
        {
            element.layout = layout;
            return element;
        }
    }
}