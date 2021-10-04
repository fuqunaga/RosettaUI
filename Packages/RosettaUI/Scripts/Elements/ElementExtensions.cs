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
            fold.IsOpen = true;
            return fold;
        }
        public static FoldElement Close(this FoldElement fold)
        {
            fold.IsOpen = false;
            return fold;
        }


        public static Element SetMinWidth(this Element element, int minWidth)
        {
            var layout = element.layout;
            layout.minWidth = minWidth;
            element.layout = layout;

            return element;
        }

        public static Element SetMinHeight(this Element element, int minHeight)
        {
            var layout = element.layout;
            layout.minHeight = minHeight;
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