using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public static class BoxShadowExtension
    {
        public static void AddBoxShadow(this VisualElement ve)
        {
            var boxShadow = new BoxShadow();
            ve.hierarchy.Insert(0, boxShadow);
            ve.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var style = boxShadow.style;
                var resolveStyle = boxShadow.resolvedStyle;
                
                style.width = evt.newRect.width + resolveStyle.borderLeftWidth + resolveStyle.borderRightWidth - 2f;
                style.height = evt.newRect.height + resolveStyle.borderTopWidth + resolveStyle.borderBottomWidth- 2f;
                
                var windowStyle = ve.resolvedStyle;

                style.marginLeft = -(resolveStyle.borderLeftWidth + windowStyle.borderLeftWidth) + 0.5f;
                style.marginTop = -(resolveStyle.borderTopWidth + windowStyle.borderTopWidth) + 0.5f;
            });
        }
        
        public static void AddBoxShadow(this GenericDropdownMenu menu)
        {
            var outerContainer = menu.contentContainer.parent.parent.parent.parent;
            outerContainer.AddBoxShadow();      
        }
    }
}