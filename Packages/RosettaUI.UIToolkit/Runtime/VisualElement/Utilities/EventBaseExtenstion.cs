using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class EventBaseExtenstion
    {
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