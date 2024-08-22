using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
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
                var resolvedStyle = boxShadow.resolvedStyle;
                
                style.width = evt.newRect.width + resolvedStyle.borderLeftWidth + resolvedStyle.borderRightWidth - 2.5f;
                style.height = evt.newRect.height + resolvedStyle.borderTopWidth + resolvedStyle.borderBottomWidth - 2.5f;
                
                var windowStyle = ve.resolvedStyle;

                style.marginLeft =  -(resolvedStyle.borderLeftWidth + windowStyle.borderLeftWidth + windowStyle.paddingLeft) + 1f;
                style.marginTop = -(resolvedStyle.borderTopWidth + windowStyle.borderTopWidth) + 1f;
            });
        }
        
        public static void AddBoxShadow(this GenericDropdownMenu menu)
        {
            var outerContainer = menu.contentContainer.parent.parent.parent.parent;
            outerContainer.AddBoxShadow();      
        }
    }
}