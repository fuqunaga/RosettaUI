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

        public static Element SetWidth(this Element element, float width)
        {
            var style = element.Style;
            if (style.width != width)
            {
                style.width = width;
                element.Style = style;
            }

            return element;
        }

        public static Element SetHeight(this Element element, float height)
        {
            var style = element.Style;
            if (style.height != height)
            {
                style.height = height;
                element.Style = style;
            }

            return element;
        }

        public static Element SetMinWidth(this Element element, float minWidth)
        {
            var style = element.Style;
            if (style.minWidth != minWidth)
            {
                style.minWidth = minWidth;
                element.Style = style;
            }

            return element;
        }

        public static Element SetMinHeight(this Element element, float minHeight)
        {
            var style = element.Style;
            if (style.minHeight != minHeight)
            {
                style.minHeight = minHeight;
                element.Style = style;
            }

            return element;
        }
        
        public static Element SetColor(this Element element, Color color)
        {
            var style = element.Style;
            if (style.color != color)
            {
                style.color = color;
                element.Style = style;
            }

            return element;
        }


        public static Element SetJustify(this Element element, Style.Justify justify)
        {
            var style = element.Style;
            if (style.justify != justify)
            {
                style.justify = justify;
                element.Style = style;
            }

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