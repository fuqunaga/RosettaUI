using System;
using UnityEngine;

namespace RosettaUI
{
    public static class ElementExtensionsMethodChain
    {
        public static T SetEnable<T>(this T e, bool enable)
            where T : Element
        {
            e.Enable = enable;
            return e;
        }

        public static T SetInteractable<T>(this T e, bool interactable)
            where T : Element
        {
            e.Interactable = interactable;
            return e;
        }

        public static T SetWidth<T>(this T element, float width)
            where T : Element
        {
            element.Style.width = width;
            return element;
        }

        public static T SetHeight<T>(this T element, float height)
            where T : Element
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

        public static Element RegisterValueChangeCallback(this Element element, Action onValueChanged)
        {
            element.onViewValueChanged += onValueChanged;
            return element; 
        }
        public static Element UnregisterValueChangeCallback(this Element element, Action onValueChanged)
        {
            element.onViewValueChanged -= onValueChanged;
            return element;
        }

        public static FoldElement SetOpenFlag(this FoldElement fold, bool flag)
        {
            fold.IsOpen = flag;
            return fold;
        }

        public static FoldElement Open(this FoldElement fold) => fold.SetOpenFlag(true);

        public static FoldElement Close(this FoldElement fold) => fold.SetOpenFlag(false);


    }
}