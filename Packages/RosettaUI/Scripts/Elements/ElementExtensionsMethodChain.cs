using System;
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
            element.Style.width = width;
            return element;
        }

        public static Element SetHeight(this Element element, float height)
        {
            element.Style.height = height;
            return element;
        }

#if false
        public static Element SetMinWidth(this Element element, float minWidth)
        {
            element.Style.minWidth = minWidth;
            return element;
        }

        public static Element SetMinHeight(this Element element, float minHeight)
        {
            element.Style.minHeight = minHeight;
            return element;
        }
        
        public static Element SetMaxWidth(this Element element, float maxWidth)
        {
            element.Style.maxWidth = maxWidth;
            return element;
        }

        public static Element SetMaxHeight(this Element element, float maxHeight)
        {
            element.Style.maxHeight = maxHeight;
            return element;
        }
#endif        

        public static Element SetColor(this Element element, Color color)
        {
            element.Style.color = color;
            return element;
        }


        public static Element SetJustify(this Element element, Style.Justify justify)
        {
            element.Style.justify = justify;
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