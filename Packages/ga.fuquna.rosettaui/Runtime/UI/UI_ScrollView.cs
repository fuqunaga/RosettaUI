using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static partial class UI
    {
        public static ScrollViewElement ScrollViewVertical(float? height, params Element[] elements) 
            => ScrollViewVertical(height, elements.AsEnumerable());

        public static ScrollViewElement ScrollViewVertical(float? height, IEnumerable<Element> elements)
            => ScrollView(ScrollViewType.Vertical, elements).SetHeight(height);

        public static ScrollViewElement ScrollViewHorizontal(float? width, params Element[] elements) 
            => ScrollViewHorizontal(width, elements.AsEnumerable());
        
        public static ScrollViewElement ScrollViewHorizontal(float? width, IEnumerable<Element> elements)
            => ScrollView(ScrollViewType.Horizontal, elements).SetWidth(width);
        
        public static ScrollViewElement ScrollViewVerticalAndHorizontal(float? width, float? height, params Element[] elements) 
            => ScrollViewVerticalAndHorizontal(width, height, elements.AsEnumerable());
        
        public static ScrollViewElement ScrollViewVerticalAndHorizontal(float? width, float? height, IEnumerable<Element> elements)
            => ScrollView(ScrollViewType.VerticalAndHorizontal, elements).SetWidth(width).SetHeight(height);
        
        
        public static ScrollViewElement ScrollView(ScrollViewType scrollViewType, IEnumerable<Element> elements) 
            => new(elements, scrollViewType);
    }
}