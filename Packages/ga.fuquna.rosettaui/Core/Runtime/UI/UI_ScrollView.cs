using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static partial class UI
    {
        public static ScrollViewElement ScrollViewVertical(float? maxHeight, params Element[] elements) 
            => ScrollViewVertical(maxHeight, elements.AsEnumerable());

        public static ScrollViewElement ScrollViewVertical(float? maxHeight, IEnumerable<Element> elements)
            => ScrollView(ScrollViewType.Vertical, elements).SetMaxHeight(maxHeight);

        public static ScrollViewElement ScrollViewHorizontal(float? maxWidth, params Element[] elements) 
            => ScrollViewHorizontal(maxWidth, elements.AsEnumerable());
        
        public static ScrollViewElement ScrollViewHorizontal(float? maxWidth, IEnumerable<Element> elements)
            => ScrollView(ScrollViewType.Horizontal, elements).SetMaxWidth(maxWidth);
        
        public static ScrollViewElement ScrollViewVerticalAndHorizontal(float? maxWidth, float? maxHeight, params Element[] elements) 
            => ScrollViewVerticalAndHorizontal(maxWidth, maxHeight, elements.AsEnumerable());
        
        public static ScrollViewElement ScrollViewVerticalAndHorizontal(float? maxWidth, float? maxHeight, IEnumerable<Element> elements)
            => ScrollView(ScrollViewType.VerticalAndHorizontal, elements).SetMaxWidth(maxWidth).SetMaxHeight(maxHeight);
        
        
        public static ScrollViewElement ScrollView(ScrollViewType scrollViewType, IEnumerable<Element> elements) 
            => new(elements, scrollViewType);
    }
}