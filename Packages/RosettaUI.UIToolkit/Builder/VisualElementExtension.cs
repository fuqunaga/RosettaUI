using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public static class VisualElementExtension
    {
        public static void ScheduleToUseResolvedLayoutBeforeRendering(this VisualElement ve, Action action)
        {
            ve.RegisterCallback<GeometryChangedEvent>(Callback);

            void Callback(GeometryChangedEvent _)
            {
                ve.UnregisterCallback<GeometryChangedEvent>(Callback);
                action();
            }
        }

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
    }
}