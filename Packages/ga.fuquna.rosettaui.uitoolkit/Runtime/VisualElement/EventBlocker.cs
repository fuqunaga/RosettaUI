using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// ModalWindowなどの下に敷いて既存のUIへのイベントをブロックするためのVisualElement
    /// </summary>
    public class EventBlocker : VisualElement
    {
        private const string USSClassName = "rosettaui-event-blocker";
        
        public EventBlocker()
        {
            AddToClassList(USSClassName);
        }
    }
}