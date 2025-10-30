using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_ScrollView(Element element, VisualElement visualElement)
        {
            if (element is not ScrollViewElement scrollViewElement ||
                visualElement is not ScrollView scrollView) return false;
            
            scrollView.mode = GetScrollViewMode(scrollViewElement.type);

            return Bind_ElementGroupContents(scrollViewElement, scrollView);
            
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