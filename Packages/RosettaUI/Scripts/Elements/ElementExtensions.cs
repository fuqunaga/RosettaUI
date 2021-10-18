namespace RosettaUI
{
    public static class ElementExtensionsMethodChain
    {
        public static Element SetEnable(this Element e, bool enable)
        {
            e.Enable = enable;
            return e;
        }

        public static Element SetInteractable(this Element e, bool interactable)
        {
            e.Interactable = interactable;
            return e;
        }

        public static Element SetMinWidth(this Element element, int minWidth)
        {
            var layout = element.Layout;
            layout.minWidth = minWidth;
            element.Layout = layout;

            return element;
        }

        public static Element SetMinHeight(this Element element, int minHeight)
        {
            var layout = element.Layout;
            layout.minHeight = minHeight;
            element.Layout = layout;

            return element;
        }

        public static Element SetJustify(this Element element, Layout.Justify justify)
        {
            var layout = element.Layout;
            layout.justify = justify;
            element.Layout = layout;
            return element;
        }

        public static Element SetLayout(this Element element, Layout layout)
        {
            element.Layout = layout;
            return element;
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


    }
}