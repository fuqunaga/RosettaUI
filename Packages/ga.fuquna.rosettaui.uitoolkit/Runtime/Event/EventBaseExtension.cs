using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class EventBaseExtension
    {
        public static bool TryGetPosition(this EventBase evt, out Vector2 position)
        {
            switch(evt)
            {
                case IPointerEvent pointerEvent:
                    position = pointerEvent.position;
                    return true;
                
                case IMouseEvent mouseEvent:
                    position = mouseEvent.mousePosition;
                    return true;
                
                default:
                    position = default;
                    return false;
            }
        }
        
        public static void StopPropagationAndFocusControllerIgnoreEvent(this EventBase evt)
        {
#if UNITY_2023_1_OR_NEWER
            evt.StopPropagation();
            if (evt.target is VisualElement targetElement)
            {
                targetElement.focusController?.IgnoreEvent(evt);   
            }
#else
            evt.PreventDefault();
#endif
        }
    }
}