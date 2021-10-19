using UnityEngine;

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


        public static Element SetColor(this Element element, Color color)
        {
            var style = element.Style;
            style.color = color;
            element.Style = style;

            return element;
        }
        
        public static Element SetMinWidth(this Element element, int minWidth)
        {
            var style = element.Style;
            style.minWidth = minWidth;
            element.Style = style;

            return element;
        }

        public static Element SetMinHeight(this Element element, int minHeight)
        {
            var style = element.Style;
            style.minHeight = minHeight;
            element.Style = style;

            return element;
        }

        public static Element SetJustify(this Element element, Style.Justify justify)
        {
            var style = element.Style;
            style.justify = justify;
            element.Style = style;
            return element;
        }

        public static Element SetStyle(this Element element, Style style)
        {
            element.Style = style;
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