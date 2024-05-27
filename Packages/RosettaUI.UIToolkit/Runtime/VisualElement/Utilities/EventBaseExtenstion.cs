using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class EventBaseExtenstion
    {
        public static void StopPropagationAndFocusControllerIgnoreEvent(this EventBase evt)
        {
            evt.StopPropagation();
            if (evt.target is VisualElement targetElement)
            {
                targetElement.focusController?.IgnoreEvent(evt);   
            }
        }
    }
}