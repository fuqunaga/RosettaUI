using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        VisualElement Build_ScrollView(Element element)
        {
            var scrollViewElement = (ScrollViewElement) element;
            var scrollViewMode = GetScrollViewMode(scrollViewElement.type);

            var scrollView = new ScrollView(scrollViewMode);
            return Build_ElementGroupContents(scrollView, element);


            static ScrollViewMode GetScrollViewMode(ScrollViewType type)
            {
                return type switch
                {
                    ScrollViewType.Vertical => ScrollViewMode.Vertical,
                    ScrollViewType.Horizontal => ScrollViewMode.Horizontal,
                    ScrollViewType.VerticalAndHorizontal => ScrollViewMode.VerticalAndHorizontal,
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
            }
        }
    }
}